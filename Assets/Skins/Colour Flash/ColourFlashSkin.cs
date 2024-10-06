public class ColourFlashSkin : ModuleSkin
{
    public override string ModuleId { get { return "ColourFlash"; } }

    public override string Name { get { return "Modern"; } }

    protected override void OnStart()
    {
        Log("Start");
    }
    protected override void OnSolve()
    {
        Log("Solve");
    }
}
