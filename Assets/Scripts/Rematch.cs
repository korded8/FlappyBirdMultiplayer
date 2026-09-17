namespace RematchManaging
{
    public class Rematch
    {
        public static bool PendingRematch = false;
        public static string RoomCode = string.Empty;
        public static bool PreferMaster = false;

        public static void Clear()
        {
            PendingRematch = false;
            RoomCode = string.Empty;
            PreferMaster = false;
        }
    }
}
