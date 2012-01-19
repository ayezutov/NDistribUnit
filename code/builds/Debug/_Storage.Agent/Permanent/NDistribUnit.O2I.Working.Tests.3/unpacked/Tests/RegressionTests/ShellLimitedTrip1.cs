using CommonTests;
using CommonTests.XmlCompare;
using NUnit.Framework;

using TRR.O2I.Common.Core;
using TRR.O2I.UI.Test;
using TRR.O2I.UI.Test.Core;

namespace TRR.O2I.UI.XMLComparisonTests
{
	[TestFixture]
	
	[RunNewBuyerSetupWorkflow]
	public class ShellLimitedTrip1 : RegressionTestsBaseWithChangeOrder
	{
		protected override string ChangeOrderXml
		{
			get { return RegressionTemplates.ShellLimited_CO_TRIPS1; }
		}

		protected override string PurchaseOrderXml
		{
			get { return RegressionTemplates.ShellLimited_PO_TRIPS1; }
		}

		protected override string PurchaseOrderResponseXml
		{
			get { return RegressionTemplates.ShellLimited_POR0_TRIPS1; }
		}

		protected static string PurchaseOrderResponse1Xml
		{
			get { return RegressionTemplates.ShellLimited_POR1_TRIPS1; }
		}

		protected override string TestFixtureName
		{
			get { return "ShellLimitedTrip1"; }
		}

		[Test]
		public void SaveAndSendPOR0()
		{
			SaveAndSendPurchaseOrder();
		}

		[Test, Category(TestCategory.PDF)]
		public void PrintPORPDFFromDetailsPage()
		{
			SearchAndSavePor();
			Site.PoDetailsPage.DownloadPorPdfIntoFolder(TestFixtureName);
			Assert.That(Site.IsFailureMessageDisplayed, Is.False, "Print POR O2I style failed.");
		}

		[Test, Category(TestCategory.XML)]
		public void DownloadAndComparePOR1XMLFromDetailsPage()
		{
			SearchAndSavePor();
			DownloadedFile file = Site.PoDetailsPage.ExportXmlIntoSpesializedFolder(DocumentType.typeOrderResponse, TestFixtureName);
			Assert.That(Site.IsFailureMessageDisplayed, Is.False, "Download Xml POR failed.");
			XmlDocumentAssert.AssertPorAreEqaul(PurchaseOrderResponse1Xml, file.ContentAsString, purchaseOrder.BuyerOrderNumber, Buyer.Ident, Supplier.Ident);
		}

		[Test, Category(TestCategory.XML)]
		public void DownloadAndComparePOR1XMLFromSearchPage()
		{
			SearchAndSavePor();
			Site.PoManagementPage.Open();
			SearchPurchaseOrder(purchaseOrder.BuyerOrderNumber, purchaseOrder.AccountCode);
			DownloadedFile file = Site.PoManagementPage.ExportXmlIntoSpecializedFolder(DocumentType.typeOrderResponse, TestFixtureName);
			Assert.That(Site.IsFailureMessageDisplayed, Is.False, "Download Xml POR failed.");
			XmlDocumentAssert.AssertPorAreEqaul(PurchaseOrderResponse1Xml, file.ContentAsString, purchaseOrder.BuyerOrderNumber, Buyer.Ident, Supplier.Ident);
		}

		private void SearchAndSavePor()
		{
			SearchPurchaseOrder(purchaseOrder.BuyerOrderNumber, purchaseOrder.AccountCode);
			Site.PoManagementPage.OpenPo(0);
			
			EnterNewData();

			Site.PoDetailsPage.Send();
			Site.PoDetailsPage.SaveDraft();
			Site.PoDetailsPage.Send();
		}

		private void EnterNewData()
		{
			Site.PoDetailsPage.ItemsGrid.Paging.ToNextLink.Click();

			Site.PoDetailsPage.ItemsGrid.Rows[0].UnitPrice.TypeTextViaValue("9.05");
			Site.PoDetailsPage.ItemsGrid.Rows[1].UnitPrice.TypeTextViaValue("81.34");
			Site.PoDetailsPage.ItemsGrid.Rows[2].UnitPrice.TypeTextViaValue("97.03");
			Site.PoDetailsPage.ItemsGrid.Rows[3].UnitPrice.TypeTextViaValue("122.59");
			Site.PoDetailsPage.ItemsGrid.Rows[4].UnitPrice.TypeTextViaValue("4.36");
			Site.PoDetailsPage.ItemsGrid.Rows[5].UnitPrice.TypeTextViaValue("10.77");

			Site.PoDetailsPage.FillBuyerToParty(@"item #1 we will supply PART # HUBBELL RS603PW, item #2 we will supply Part # NP26, Item #3 we will supply part # 60T10CL/3M/130V/STD, item #5 part # will be BUS GMA125MA, item # 9 part # will be BUS GMA250MA, item # 12 part # will be ALB 1497DCXSX3N, Item # 13 part # ALB 1497ECXSX3N, item #14 part # ALB 1497FCXSX3N, Item # 5 Part # BUS BC6031SQ");
		}
	}
}