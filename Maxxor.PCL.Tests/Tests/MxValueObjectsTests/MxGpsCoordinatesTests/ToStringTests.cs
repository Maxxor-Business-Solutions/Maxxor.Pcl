﻿using System;
using System.Globalization;
using Maxxor.PCL.Tests.Tests.Base;
using Maxxor.PCL.ValueObject;
using NUnit.Framework;

namespace Maxxor.PCL.Tests.Tests.MxValueObjectsTests.MxGpsCoordinatesTests
{
    [TestFixture]
    public class ToStringTests : BaseUnitTest
    {
        [Test]
        public void WHEN_IsValidCoordinates_SHOULD_return_LatLong_seperated_by_comma()
        {
            //Arrange
            const double latitude = 65.564865;
            const double longitude = 33.45659;
            var location = new MxGpsCoordinates(latitude, longitude);

            //Act
            var result = location.ToString();

            //Assert
            Assert.AreEqual("65.564865,33.45659", result);
        }
        [Test]
        public void WHEN_IsNOTValidCoordinates_SHOULD_return_Empty_String()
        {
            //Arrange
            const double latitude = 1165.564865;
            const double longitude = 33.45659;
            var location = new MxGpsCoordinates(latitude, longitude);

            //Act
            var result = location.ToString();

            //Assert
            Assert.That(string.IsNullOrEmpty(result));
        }
    }
}
