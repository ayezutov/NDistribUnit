using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Autofac;
using Moq;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Logging;
using NUnit.Framework;
using System.Xml.XPath;
using Vestris.ResourceLib;
using IContainer = Autofac.IContainer;

namespace NDistribUnit.Integration.Tests.Tests
{
    [TestFixture]
    public class TemporaryTestForPlaying
    {
        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public void Dispose()
        {
        }

        [TestFixtureSetUp]
        public void InitOnce()
        {
        }

        [TestFixtureTearDown]
        public void DisposeOnce()
        {
        }


        [Serializable]
        class Foo : IDeserializationCallback
        {
            public Dictionary<int, string> Dict { get; private set; }

            public Foo()
            {
                Dict = new Dictionary<int, string>();
            }
            
            public void OnDeserialization(object sender)
            {
                Dict.OnDeserialization(sender);
                Dict.Add(99, "test"); // Error here
            }
        }

        [Test]
        public void DocParsing()
        {
            var doc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <xml>
            <root>
            <Item>
            <taxids>
                <string>1</string>
                <string>374</string>
                <string>723</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            <taxids>
                <string>2-</string>
                <string>374</string>
                <string>724</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            <taxids>
                <string>3</string>
                <string>374</string>
                <string>723</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            </Item>
            <Item>
            <taxids>
                <string>4</string>
                <string>374</string>
                <string>723</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            <taxids>
                <string>5-</string>
                <string>374</string>
                <string>724</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            <taxids>
                <string>6</string>
                <string>374</string>
                <string>723</string>
                <string>1087</string>
                <string>1118</string>
                <string>1121</string>
            </taxids>
            </Item>
            </root>
            </xml>");

            var values = from ids in doc.XPathSelectElements("/xml/root/Item/taxids")
                         from id in ids.Elements("string")
                         where id.Value.Contains("723")
                         select ids.ToString();

            var result = string.Join("\n", values);

            Console.WriteLine(result);
        }

        [Test]
        public void SerializationTest()
        {
            var formatter = new BinaryFormatter();

            var foo = new Foo();
            foo.Dict.Add(1, "456");

            var memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, foo);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var foo2 = (Foo)formatter.Deserialize(memoryStream);
            
        }

        [Test, Explicit]
        public void Exceptions()
        {
            try
            {
                Console.WriteLine("Before kill");
                Environment.Exit(0);
                Console.WriteLine("After kill");
            }
            catch (A a)
            {
                Console.WriteLine("Catch");
                throw new B();
            }
            finally
            {
                Console.WriteLine("Finally");
                throw new C();
            }
        }

        private class A : Exception
        {
        }

        private class B : Exception
        {
        }

        private class C : Exception
        {
        }

        [Test]
        public void CanBuildMultipleContainers()
        {
            var b = new ContainerBuilder();

            b.RegisterType<ConsoleLog>().As<ILog>();

            IContainer container = b.Build();

            Assert.That(container.Resolve<ILog>(), Is.InstanceOf<ConsoleLog>());

            var b2 = new ContainerBuilder();
            b2.Register(c => new RollingLog(1000)).As<ILog>();
            b2.Update(container);

            Assert.That(container.Resolve<ILog>(), Is.InstanceOf<RollingLog>());

            var logs = container.Resolve<IEnumerable<ILog>>();

            Assert.That(logs.Count(), Is.EqualTo(2));
        }

        [Test, Explicit]
        public void TryGetInstructions()
        {
            Regex typeIdentifier = new Regex(@"^\s*(?:NDistribUnit\.)(?<selector>\w+)\s*(?<object>{.+})\s*$", RegexOptions.Compiled);

            var fileName = @"d:\work\personal\NDistribUnit\test\O2ITestsDebug2_dec_2011Debug\UI.Test.dll.config";
            var doc = XDocument.Load(fileName);


            Console.WriteLine(doc.ToString());
        }

        [Test]
        public void OptionalParametersCanBeIgnored()
        {
            var b = new ContainerBuilder();

            b.RegisterType<ConsoleLog>().As<ILog>().AsSelf();
            b.RegisterType<OptionalParamsInConstructor>();

            IContainer container = b.Build();

            Assert.That(container.Resolve<OptionalParamsInConstructor>().RollingLog, Is.Null);
        }

        [Test]
        public void AnotherMockWorks()
        {
            var builder = new ContainerBuilder();

            var vp = new Mock<IVersionProvider>();
            vp.Setup(v => v.GetVersion()).Returns(new Version("1.0.0.0"));

            builder.RegisterInstance(vp.Object).As<IVersionProvider>();

            var container = builder.Build();

            var versionProvider1 = container.Resolve<IVersionProvider>();

            var builder2 = new ContainerBuilder();

            var vp2 = new Mock<IVersionProvider>();
            vp2.Setup(v => v.GetVersion()).Returns(new Version("2.0.0.0"));

            builder2.RegisterInstance(vp2.Object).As<IVersionProvider>();

            builder2.Update(container);

            var versionProvider2 = container.Resolve<IVersionProvider>();

            Assert.That(versionProvider2.GetVersion(), Is.EqualTo(new Version("2.0.0.0")));
            Assert.That(versionProvider1.GetVersion(), Is.EqualTo(new Version("1.0.0.0")));
        }

        [Test]
        public void ToExcelConverter_base_2()
        {
            var converter = new RowIndexToExcelLetterConverter(@base: 2);

            var sb = new StringBuilder();
            for (int i = 1; i <= 15; i++)
            {
                sb.AppendFormat(" {0}", converter.Convert(i));
            }

            Assert.That(sb.ToString(),
                        Is.EqualTo(" A B AA AB BA BB AAA AAB ABA ABB BAA BAB BBA BBB AAAA"));
        }

        [Test]
        public void ToExcelConverter_base_3()
        {
            var converter = new RowIndexToExcelLetterConverter(@base: 3);

            var sb = new StringBuilder();
            for (int i = 1; i <= 40; i++)
            {
                sb.AppendFormat(" {0}", converter.Convert(i));
            }

            Assert.That(sb.ToString(),
                        Is.EqualTo(
                            " A B C AA AB AC BA BB BC CA CB CC AAA AAB AAC ABA ABB ABC ACA ACB ACC BAA BAB BAC BBA BBB BBC BCA BCB BCC CAA CAB CAC CBA CBB CBC CCA CCB CCC AAAA"));
        }


        [Test]
        public void ToExcelConverter_base_26()
        {
            var converter = new RowIndexToExcelLetterConverter(@base: 26);

            var sb = new StringBuilder();
            for (int i = 1; i <= 65536; i++)
            {
                sb.AppendFormat(" {0}", converter.Convert(i));
            }

            Console.WriteLine(sb.ToString());

            Assert.That(
                sb.ToString().StartsWith(
                    " A B C D E F G H I J K L M N O P Q R S T U V W X Y Z AA AB AC AD AE AF AG AH AI AJ AK AL AM AN AO AP AQ AR AS AT AU AV AW AX AY AZ BA BB BC BD BE BF BG BH BI BJ BK BL BM BN BO BP BQ BR BS BT BU BV BW BX BY BZ CA CB CC CD CE CF CG CH CI CJ CK CL CM CN CO CP CQ CR CS CT CU CV CW CX CY CZ DA DB DC DD DE DF DG DH DI DJ DK DL DM DN DO DP DQ DR DS DT DU DV DW DX DY DZ EA EB EC ED EE EF EG EH EI EJ EK EL EM EN EO EP EQ ER ES ET EU EV EW EX EY EZ FA FB FC FD FE FF FG FH FI FJ FK FL FM FN FO FP FQ FR FS FT FU FV FW FX FY FZ GA GB GC GD GE GF GG GH GI GJ GK GL GM GN GO GP GQ GR GS GT GU GV GW GX GY GZ HA HB HC HD HE HF HG HH HI HJ HK HL HM HN HO HP HQ HR HS HT HU HV HW HX HY HZ IA IB IC ID IE IF IG IH II IJ IK IL IM IN IO IP IQ IR IS IT IU IV IW IX IY IZ JA JB JC JD JE JF JG JH JI JJ JK JL JM JN JO JP JQ JR JS JT JU JV JW JX JY JZ KA KB KC KD KE KF KG KH KI KJ KK KL KM KN KO KP KQ KR KS KT KU KV KW KX KY KZ LA LB LC LD LE LF LG LH LI LJ LK LL LM LN LO LP LQ LR LS LT LU LV LW LX LY LZ MA MB MC MD ME MF MG MH MI MJ MK ML MM MN MO MP MQ MR MS MT MU MV MW MX MY MZ NA NB NC ND NE NF NG NH NI NJ NK NL NM NN NO NP NQ NR NS NT NU NV NW NX NY NZ OA OB OC OD OE OF OG OH OI OJ OK OL OM ON OO OP OQ OR OS OT OU OV OW OX OY OZ PA PB PC PD PE PF PG PH PI PJ PK PL PM PN PO PP PQ PR PS PT PU PV PW PX PY PZ QA QB QC QD QE QF QG QH QI QJ QK QL QM QN QO QP QQ QR QS QT QU QV QW QX QY QZ RA RB RC RD RE RF RG RH RI RJ RK RL RM RN RO RP RQ RR RS RT RU RV RW RX RY RZ SA SB SC SD SE SF SG SH SI SJ SK SL SM SN SO SP SQ SR SS ST SU SV SW SX SY SZ TA TB TC TD TE TF TG TH TI TJ TK TL TM TN TO TP TQ TR TS TT TU TV TW TX TY TZ UA UB UC UD UE UF UG UH UI UJ UK UL UM UN UO UP UQ UR US UT UU UV UW UX UY UZ VA VB VC VD VE VF VG VH VI VJ VK VL VM VN VO VP VQ VR VS VT VU VV VW VX VY VZ WA WB WC WD WE WF WG WH WI WJ WK WL WM WN WO WP WQ WR WS WT WU WV WW WX WY WZ XA XB XC XD XE XF XG XH XI XJ XK XL XM XN XO XP XQ XR XS XT XU XV XW XX XY XZ YA YB YC YD YE YF YG YH YI YJ YK YL YM YN YO YP YQ YR YS YT YU YV YW YX YY YZ ZA ZB ZC ZD ZE ZF ZG ZH ZI ZJ ZK ZL ZM ZN ZO ZP ZQ ZR ZS ZT ZU ZV ZW ZX ZY ZZ AAA"));
        }

        public class RowIndexToExcelLetterConverter
        {
            private readonly char one;
            private readonly int @base;

            public RowIndexToExcelLetterConverter(int @base, char one = 'A')
            {
                this.one = one;
                this.@base = @base;
            }

            public string Convert(int index)
            {
                var result = new StringBuilder();

                while (index > 0)
                {
                    int remainder;
                    index = Math.DivRem(index - 1, @base, out remainder);

                    result.Insert(0, (char)(one + remainder));
                }
                return result.ToString();
            }
        }

        [Test]
        public void ChangeVersion()
        {
            var sourceFile =
                @"d:\work\personal\NDistribUnit\source\builds\Debug\Fixed.Version\Server\NDistribUnit.Server.exe";

            var targetFile =
                @"d:\work\personal\NDistribUnit\source\builds\Debug\Server.exe";

            VersionResource targetVersion;

            using (var sourceInfo = new ResourceInfo())
            {
                using (var targetInfo = new ResourceInfo())
                {
                    try
                    {
                        sourceInfo.Load(sourceFile);
                        targetInfo.Load(targetFile);
                    }
                    catch (Win32Exception)
                    {
                        //                        if (ContinueOnError)
                        //                            return true;
                        throw;
                    }

                    VersionResource sourceVersion = sourceInfo.OfType<VersionResource>().FirstOrDefault();

                    targetVersion = targetInfo.OfType<VersionResource>().FirstOrDefault();

                    var valuesToCopy = new[] { "FileDescription", "InternalName" };

                    StringTable sourceDefaultStringTable = ((StringFileInfo)(sourceVersion["StringFileInfo"])).Default;
                    StringTable targetDefaultStringTable = ((StringFileInfo)(targetVersion["StringFileInfo"])).Default;

                    foreach (var value in valuesToCopy)
                    {
                        targetDefaultStringTable.Strings[value].Value = sourceDefaultStringTable.Strings[value].Value;
                    }
                }
            }

            targetVersion.SaveTo(targetFile);

        }

        private class OptionalParamsInConstructor
        {
            public ConsoleLog ConsoleLog { get; set; }
            public RollingLog RollingLog { get; set; }

            public OptionalParamsInConstructor(ConsoleLog consoleLog, RollingLog rollingLog = null)
            {
                ConsoleLog = consoleLog;
                RollingLog = rollingLog;
            }
        }
    }
}