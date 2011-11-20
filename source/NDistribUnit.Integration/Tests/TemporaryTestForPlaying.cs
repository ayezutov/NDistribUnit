﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Autofac;
using Moq;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Integration.Tests.Infrastructure;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.SampleTestAssembly.CategorizedTests;
using NUnit.Framework;

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

        [Test]
        public void TestResultSerializationTest()
        {
            string targetAssembly = typeof(TestFixtureWithCategoriesOnTests).Assembly.Location;
            var project = new TestProject(Path.GetDirectoryName(targetAssembly));

            var runner = new AgentTestRunner(null, new NativeRunnerCache(), new NUnitInitializer(), new ConsoleLog());
            var run = new TestRun
            {
                NUnitParameters = new NUnitParameters
                {
                    AssembliesToTest = new List<string> { targetAssembly }
                }
            };


            var result = runner.GetNUnitTestResult(new TestUnit(run, null, true, "TestSuite", targetAssembly), project);
            

            
            var system = new NDistribUnitTestSystem();
            

            

            var serializer = new DataContractSerializer(result.GetType(), new []
                                                                              {
                                                                                  typeof(ArrayList)
                                                                              });
            var stream = new MemoryStream();
            serializer.WriteObject(stream, result);
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
                Is.EqualTo(" A B C AA AB AC BA BB BC CA CB CC AAA AAB AAC ABA ABB ABC ACA ACB ACC BAA BAB BAC BBA BBB BBC BCA BCB BCC CAA CAB CAC CBA CBB CBC CCA CCB CCC AAAA"));
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

            Assert.That(sb.ToString().StartsWith(" A B C D E F G H I J K L M N O P Q R S T U V W X Y Z AA AB AC AD AE AF AG AH AI AJ AK AL AM AN AO AP AQ AR AS AT AU AV AW AX AY AZ BA BB BC BD BE BF BG BH BI BJ BK BL BM BN BO BP BQ BR BS BT BU BV BW BX BY BZ CA CB CC CD CE CF CG CH CI CJ CK CL CM CN CO CP CQ CR CS CT CU CV CW CX CY CZ DA DB DC DD DE DF DG DH DI DJ DK DL DM DN DO DP DQ DR DS DT DU DV DW DX DY DZ EA EB EC ED EE EF EG EH EI EJ EK EL EM EN EO EP EQ ER ES ET EU EV EW EX EY EZ FA FB FC FD FE FF FG FH FI FJ FK FL FM FN FO FP FQ FR FS FT FU FV FW FX FY FZ GA GB GC GD GE GF GG GH GI GJ GK GL GM GN GO GP GQ GR GS GT GU GV GW GX GY GZ HA HB HC HD HE HF HG HH HI HJ HK HL HM HN HO HP HQ HR HS HT HU HV HW HX HY HZ IA IB IC ID IE IF IG IH II IJ IK IL IM IN IO IP IQ IR IS IT IU IV IW IX IY IZ JA JB JC JD JE JF JG JH JI JJ JK JL JM JN JO JP JQ JR JS JT JU JV JW JX JY JZ KA KB KC KD KE KF KG KH KI KJ KK KL KM KN KO KP KQ KR KS KT KU KV KW KX KY KZ LA LB LC LD LE LF LG LH LI LJ LK LL LM LN LO LP LQ LR LS LT LU LV LW LX LY LZ MA MB MC MD ME MF MG MH MI MJ MK ML MM MN MO MP MQ MR MS MT MU MV MW MX MY MZ NA NB NC ND NE NF NG NH NI NJ NK NL NM NN NO NP NQ NR NS NT NU NV NW NX NY NZ OA OB OC OD OE OF OG OH OI OJ OK OL OM ON OO OP OQ OR OS OT OU OV OW OX OY OZ PA PB PC PD PE PF PG PH PI PJ PK PL PM PN PO PP PQ PR PS PT PU PV PW PX PY PZ QA QB QC QD QE QF QG QH QI QJ QK QL QM QN QO QP QQ QR QS QT QU QV QW QX QY QZ RA RB RC RD RE RF RG RH RI RJ RK RL RM RN RO RP RQ RR RS RT RU RV RW RX RY RZ SA SB SC SD SE SF SG SH SI SJ SK SL SM SN SO SP SQ SR SS ST SU SV SW SX SY SZ TA TB TC TD TE TF TG TH TI TJ TK TL TM TN TO TP TQ TR TS TT TU TV TW TX TY TZ UA UB UC UD UE UF UG UH UI UJ UK UL UM UN UO UP UQ UR US UT UU UV UW UX UY UZ VA VB VC VD VE VF VG VH VI VJ VK VL VM VN VO VP VQ VR VS VT VU VV VW VX VY VZ WA WB WC WD WE WF WG WH WI WJ WK WL WM WN WO WP WQ WR WS WT WU WV WW WX WY WZ XA XB XC XD XE XF XG XH XI XJ XK XL XM XN XO XP XQ XR XS XT XU XV XW XX XY XZ YA YB YC YD YE YF YG YH YI YJ YK YL YM YN YO YP YQ YR YS YT YU YV YW YX YY YZ ZA ZB ZC ZD ZE ZF ZG ZH ZI ZJ ZK ZL ZM ZN ZO ZP ZQ ZR ZS ZT ZU ZV ZW ZX ZY ZZ AAA"));
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