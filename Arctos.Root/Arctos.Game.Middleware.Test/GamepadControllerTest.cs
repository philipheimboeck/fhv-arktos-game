using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArctosGameServer.Input;

namespace Arctos.Game.Middleware.Test
{
    /// <summary>
    /// Tested den XBox Controller input!
    /// </summary>
    [TestClass]
    public class GamepadControllerTest
    {
        private GamepadController _controller;
        bool Finished { get; set; }

        public GamepadControllerTest()
        {
            _controller = new GamepadController();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Textkontext mit Informationen über
        ///den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
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
            GamepadControllerTest _test;

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
                System.Diagnostics.Debug.WriteLine("Key: " + (value.PressedKey != null ? value.PressedKey.ToString() : "none"));
                System.Diagnostics.Debug.WriteLine("Value: " + (value.Value != null ? value.Value.ToString() : "none"));

                // Shut down test
                if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.CONTROLLER_DISCONNECTED))
                {
                    _test.Finished = true;
                }
            }
        }
    }
}
