using System.Collections.Generic;

public class FOVSkin : ModuleSkin
{
    public override string ModuleId { get { return "forgetOurVoices"; } }
    public override string Name { get { return "TransRightsAreHumanRights"; } }

    protected override Dictionary<string, string> SoundOverrides
    {
        get
        {
            var overrides = new Dictionary<string, string>() {
                { "Solve_MQE", "ForgetOurVoices_Solve" },
                { "Strike_MQE", "ForgetOurVoices_Strike" },
            };
            for (int i = 0; i < 10; i++)
                overrides["M" + i] = "ForgetOurVoices_" + i;

            return overrides;
        }
    }
}
