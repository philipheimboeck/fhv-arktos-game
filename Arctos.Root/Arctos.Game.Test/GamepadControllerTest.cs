using System;
using Arctos.Game.ControlUnit.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arctos.Game.Middleware.Test
{
    /// <summary>
    /// Tested den XBox Controller input!
    /// </summary>
    [TestClass]
    public class GamepadControllerTest
    {
        private GamepadController _controller;

        private TestContext testContextInstance;

        public GamepadControllerTest()
        {
            _controller = new GamepadController();
        }

        private bool Finished { get; set; }

        /// <summary>
        ///Ruft den Textkontext mit Informationen über
        ///den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void GamepadTestMethod()
        {
            System.Diagnostics.Debug.WriteLine("Test started");
            var observer = new Observer(this);

            while (!Finished)
            {
                _controller.Subscribe(observer);
                _controller.Update();
            }
        }

        private class Observer : IObserver<GamepadController.GamepadControllerEvent>
        {
            private GamepadControllerTest _test;

            public Observer(GamepadControllerTest test)
            {
                _test = test;
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(GamepadController.GamepadControllerEvent value)
            {
                System.Diagnostics.Debug.WriteLine("Event: " + value.Type);
                System.Diagnostics.Debug.WriteLine("Wheels: " +
                                                   (value.PressedWheels != null ? value.PressedWheels.ToString() : "none"));
                System.Diagnostics.Debug.WriteLine("Value: " + (value.Value != null ? value.Value.ToString() : "none"));

                // Shut down test
                if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerDisconnected))
                {
                    _test.Finished = true;
                }
            }
        }

        #region Zusätzliche Testattribute

        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion
    }
}