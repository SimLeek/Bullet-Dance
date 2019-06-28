using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;


namespace Tests
{
    public class TestGroundMovement
    {

        public GameObject player;
        public GameObject plane;
        //public CharacterControl controller;

        [OneTimeSetUp]
        public void SetUpPlayerOnGround()
        {
            SceneManager.LoadScene("CharacterGroundMovementTestScene", LoadSceneMode.Single);
            player = GameObject.Find("unitychan_dynamic_locomotion");
            plane = GameObject.Find("Plane");
            //controller = player.GetComponent<CharacterControl>();
        }

        [Test]
        public void TestGroundMovementSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        [UnityTest]
        public IEnumerator TestWalkForward()
        {
            yield return null;
        }
    }
}
