using Narumikazuchi.Networking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    [TestClass]
    public class MacAddressTest
    {
        [TestMethod]
        public void MacAddressConstructor()
        {
            Byte[] address = new Byte[] { 0x02, 0xEF, 0x35, 0xAA, 0x6E, 0x2C };
            MacAddress mac = new(address);
            Assert.IsTrue(mac == address);
        }

        [TestMethod]
        public void MacAddressConstructorFailure()
        {
            Byte[] address = new Byte[] { 0x02, 0xEF, 0x35, 0xAA, 0x6E, 0x2C, 0xFF, 0xFF };
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = new MacAddress(address));
            address = new Byte[] { 0x2C, 0xFF, 0xFF };
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = new MacAddress(address));
        }

        [TestMethod]
        public void SuccessfulMacAddressParse()
        {
            String raw = "FFEB0864CE0A";
            Byte[] address = new Byte[] { 0xFF, 0xEB, 0x08, 0x64, 0xCE, 0x0A };
            MacAddress mac = MacAddress.Parse(raw);
            Assert.IsTrue(mac == address);
            raw = "FF:EB:08:64:CE:0A";
            mac = MacAddress.Parse(raw);
            Assert.IsTrue(mac == address);
            raw = "FF EB  08 64CE   0A";
            mac = MacAddress.Parse(raw);
            Assert.IsTrue(mac == address);
        }

        [TestMethod]
        public void UnsuccessfulMacAddressParse()
        {
            String raw = "FFEZ0864CE0A";
            Assert.ThrowsException<FormatException>(() => _ = MacAddress.Parse(raw));
        }

        [TestMethod]
        public void SuccessfulMacAddressTryParse()
        {
            String raw = "FFEB0864CE0A";
            Byte[] address = new Byte[] { 0xFF, 0xEB, 0x08, 0x64, 0xCE, 0x0A };
            Boolean result = MacAddress.TryParse(raw, out MacAddress? mac);
            Assert.IsTrue(result);
            Assert.IsTrue(mac == address);
            raw = "FF:EB:08:64:CE:0A";
            result = MacAddress.TryParse(raw, out mac);
            Assert.IsTrue(result);
            Assert.IsTrue(mac == address);
            raw = "FF EB  08 64CE   0A";
            result = MacAddress.TryParse(raw, out mac);
            Assert.IsTrue(result);
            Assert.IsTrue(mac == address);
        }

        [TestMethod]
        public void UnsuccessfulMacAddressTryParse()
        {
            String raw = "FFEZ0864CE0A";
            Boolean result = MacAddress.TryParse(raw, out MacAddress? mac);
            Assert.IsFalse(result);
            Assert.IsNull(mac);
        }

        [TestMethod]
        public void MacAddressEquality()
        {
            String raw = "FFEB0864CE0A";
            Byte[] address = new Byte[] { 0xFF, 0xEB, 0x08, 0x64, 0xCE, 0x0A };
            MacAddress macA = MacAddress.Parse(raw);
            MacAddress macB = new(address);
            Assert.AreEqual(macA, macB);
        }
    }
}
