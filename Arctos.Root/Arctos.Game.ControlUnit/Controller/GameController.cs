namespace ArctosGameServer.Controller
{
    /// <summary>
    /// The GameController
    /// </summary>
    public class GameController
    {
        /// <summary>
        /// Generates a new map
        /// </summary>
        public void GenerateGame()
        {
            // GameConfiguration
        }

        public void loop()
        {
            while (true)
            {
                //if RFID received
                this.UpdateArea("RFID-TAG");
            }
        }

        private void UpdateArea(string areaID)
        {
        }
    }
}