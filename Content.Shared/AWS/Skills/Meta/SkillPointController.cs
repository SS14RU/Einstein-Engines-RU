using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared.AWS.Skills;

public class SkillPointController
{
    public int MaxPoints { get; set; }
    public int CurrentPoints { get => SumEstablishedPoints(); }
    public Action<ProtoId<SkillPrototype>, SkillLevel>? OnRecalculateSkill;
    private SkillContainer Container { get; }
    private readonly IPrototypeManager _prototypeManager;

    private void CopyUnblocked(Dictionary<ProtoId<SkillPrototype>, List<Enum>> from, Dictionary<ProtoId<SkillPrototype>, List<Enum>> to)
    {
        foreach (var (key, value) in from)
            to[key] = [.. value];
    }

    public SkillLevel GetCurrentSkillLevel(ProtoId<SkillPrototype> protoId)
    {
        if (Container.Skills.TryGetValue(protoId, out var currentLevel))
            return (SkillLevel)currentLevel;

        return SkillLevel.NonSkilled;
    }

    public bool CanHaveSkillLevel(ProtoId<SkillPrototype> protoId, SkillLevel level)
    {
        if (!_prototypeManager.TryIndex(protoId, out var skillProto))
            return false;

        if (skillProto.Cost.TryGetValue(level, out _))
            return true;

        return false;
    }

    private SkillLevel GetAnyLessThan(ProtoId<SkillPrototype> protoId, SkillLevel currentSkillLevel)
    {
        if (!_prototypeManager.TryIndex(protoId, out var skillProto))
            return SkillLevel.NonSkilled;

        return GetAnyLessThan(skillProto, currentSkillLevel);
    }

    private SkillLevel GetAnyLessThan(SkillPrototype skillProto, SkillLevel currentSkillLevel)
    {
        var mostMatched = currentSkillLevel;

        foreach (var (key, value) in skillProto.Cost)
            if ((SkillLevel)key < currentSkillLevel)
                mostMatched = (SkillLevel)key;

        return mostMatched;
    }

    private SkillLevel GetAnyHigerMostMatchedThanAndLess(SkillPrototype skillProto, SkillLevel currentSkillLevel, SkillLevel selectedSkillLevel)
    {
        var currentSkillPoints = CurrentPoints;
        SkillLevel lastCycledLevel = currentSkillLevel;

        foreach (var (key, value) in skillProto.Cost)
            if (key is SkillLevel cycleLevel)
                if (cycleLevel > currentSkillLevel && cycleLevel <= selectedSkillLevel)
                    if (currentSkillPoints + GetPointsForSkill(skillProto.ID, cycleLevel) >= 0)
                        lastCycledLevel = cycleLevel;

        return lastCycledLevel;
    }

    public SkillLevel MatchMoreExistenceLevel(ProtoId<SkillPrototype> protoId, SkillLevel currentSkillLevel, SkillLevel selectedSkillLevel)
    {
        if (!_prototypeManager.TryIndex(protoId, out var skillProto))
            return SkillLevel.NonSkilled;

        if (skillProto.Cost.Count == 1)
            return (SkillLevel)skillProto.Cost.First().Key;

        if (currentSkillLevel > selectedSkillLevel)
            return GetAnyLessThan(skillProto, currentSkillLevel);

        if (selectedSkillLevel > currentSkillLevel)
            return GetAnyHigerMostMatchedThanAndLess(skillProto, currentSkillLevel, selectedSkillLevel);

        return currentSkillLevel;
    }

    private int GetPointsForSkillByCurrentSkillLevel(SkillLevel currentSkillLevel, ProtoId<SkillPrototype> protoId, SkillLevel level)
    {
        if (!_prototypeManager.TryIndex(protoId, out var skillProto))
            return 0;

        if (!skillProto.Cost.TryGetValue(level, out var selectedSkillCost))
            return 0;

        if (currentSkillLevel == level)
            return ((int)selectedSkillCost);

        var currentSkillCost = skillProto.Cost[currentSkillLevel];

        return (int)(currentSkillCost - selectedSkillCost);
    }

    public int GetPointsForSkill(ProtoId<SkillPrototype> protoId, SkillLevel level)
    {
        return GetPointsForSkillByCurrentSkillLevel(GetCurrentSkillLevel(protoId), protoId, level);
    }

    public void ProcessSkill(ProtoId<SkillPrototype> protoId, SkillLevel level)
    {
        var currentSkillLevel = GetCurrentSkillLevel(protoId);
        var moreExistenceLevel = MatchMoreExistenceLevel(protoId, currentSkillLevel, level);
        var skillCost = GetPointsForSkill(protoId, moreExistenceLevel);

        if (currentSkillLevel == SkillLevel.NonSkilled && level == SkillLevel.NonSkilled)
            return;

        var anyLess = GetAnyLessThan(protoId, currentSkillLevel);

        if (currentSkillLevel == level && currentSkillLevel != anyLess)
        {
            currentSkillLevel = anyLess;
            SetSkillLevel(protoId, currentSkillLevel);
            OnRecalculateSkill?.Invoke(protoId, currentSkillLevel);
            return;
        }

        if (CurrentPoints + skillCost >= 0)
        {
            SetSkillLevel(protoId, moreExistenceLevel);
            OnRecalculateSkill?.Invoke(protoId, moreExistenceLevel);
        }
    }

    private void SetSkillLevel(ProtoId<SkillPrototype> protoId, SkillLevel level)
        => Container.Skills[protoId] = level;

    private int SumEstablishedPoints()
    {
        int currentSkillsCost = 0;

        foreach (var (key, value) in Container.Skills)
            currentSkillsCost += GetPointsForSkill(key, (SkillLevel)value);

        return MaxPoints - currentSkillsCost;
    }

    [Obsolete("You should do this logic in your system")]
    public static (bool, SkillContainer?) IsValid(int maxPoints, Dictionary<ProtoId<SkillPrototype>, List<Enum>> unblockedSkills, SkillContainer clContainer)
    {
        var skillController = new SkillPointController(maxPoints, unblockedSkills, null, clContainer);

        if (skillController.CurrentPoints >= 0)
            return (true, skillController.Container);

        return (false, null);
    }

/*    public int CalculateAges(int age)
    {
        if (age >= _minAge)
        {
            if (age >= _ageSecondStep)
            {

            }

        }

        return 0;
    }

    public SkillPointController(int age, ProtoId<Historical.HistoryPrototype> historyId, IPrototypeManager? protoManager)
    {
        MaxPoints = CalculateAges(age);

        _prototypeManager = protoManager ?? IoCManager.Resolve<IPrototypeManager>();
    }*/

    public SkillPointController(int maxPoints, Dictionary<ProtoId<SkillPrototype>, List<Enum>> unblockedSkills, IPrototypeManager? protoManager, SkillContainer? container)
    {
        Container = container ?? new();
        MaxPoints = maxPoints;

        foreach (var (key, value) in unblockedSkills)
            Container.UnblockedSkillLevels[key] = value;

        _prototypeManager = protoManager ?? IoCManager.Resolve<IPrototypeManager>();
    }
}
