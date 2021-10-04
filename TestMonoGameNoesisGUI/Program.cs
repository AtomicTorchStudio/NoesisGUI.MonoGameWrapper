namespace TestMonoGameNoesisGUI
{
    #region

    using System;

    #endregion

    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new GameWithNoesis())
            {
                game.Run();
            }
        }
    }
}