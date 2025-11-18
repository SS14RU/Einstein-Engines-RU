using Content.Shared.Gravity;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Gravity
{
    [RegisterComponent]
    [Access(typeof(GravityGeneratorSystem), typeof(Content.Server.AWS.Gravity.AwsGravityGeneratorFieldSystem))]
    public sealed partial class GravityGeneratorComponent : SharedGravityGeneratorComponent
    {
        [DataField("lightRadiusMin")] public float LightRadiusMin { get; set; }
        [DataField("lightRadiusMax")] public float LightRadiusMax { get; set; }

        //IH - Start
        // TODO: ProtectRadius задуман для будущих проверок радиуса (солнечные лучи / локальные зоны), пока не используется.
        [DataField("protectRadius")] public float ProtectRadius { get; set; } = 48f;
        [DataField("massMultiplier")] public float MassMultiplier { get; set; } = 10f;
        [DataField("blocksFtl")] public bool BlocksFtl { get; set; } = true;
        //IH - End

        /// <summary>
        /// Is the gravity generator currently "producing" gravity?
        /// </summary>
        [ViewVariables]
        public bool GravityActive { get; set; } = false;

        //IH - Start
        [ViewVariables]
        public EntityUid? CurrentGrid { get; set; }
        //IH - End
    }
}
