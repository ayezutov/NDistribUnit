using CommonTests;
using NUnit.Framework;
using TRR.O2I.BusinessLogic.Core.Workflow;
using TRR.O2I.UI.Test;

namespace TRR.O2I.UI.XMLComparisonTests
{
	[TestFixture]
	
	[RunNewBuyerSetupWorkflow]
	public class ConsolRegressionTest3 : RegressionTestsBaseWithChangeOrder
	{
		private GoodsReceiptPageActionsBase grPageActionsBase;

		protected override string PurchaseOrderXml
		{
			get { return PurchaseOrders.PO3Consol_TRIPS22; }
		}

		protected override string PurchaseOrderResponseXml
		{
			get { return RegressionTemplates.Consol_TRIPS22_POR3; }
		}

		protected override TripName Trip
		{
			get { return TripName.T22; }
		}

		protected static string GrXml
		{
			get { return GoodsReceipts.GR3Consol_TRIPS22; }
		}

		protected override string ChangeOrderXml
		{
			get { return ChangeOrders.CO3Consol_TRIPS22; }
		}

		protected override string TestFixtureName
		{
			get { return "ConsolRegressionTest3"; }
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			grPageActionsBase = new GoodsReceiptPageActionsBase(Site, Trip, GrXml, purchaseOrder, Buyer, Supplier, TestFixtureName);
		}

		[Test]
		public void ShredAndSearchGr()
		{
			grPageActionsBase.ShredAndFindGoodsReceipt();
		}

		[Test]
		public void CreateAndSentInvoiceFromGRDetailsPage()
		{
			grPageActionsBase.CreateAndSendInvoiceFromGoodsReceiptDetailsPage();
		}

		[Test]
		public void CreateAndSentInvoiceFromGRFromBasket()
		{
			grPageActionsBase.CreateAndSendInvoiceFromGoodsReceiptFromBasket();
		}

		[Test, Category(TestCategory.XML)]
		public void DownloadGRXmlFromDetailsPage()
		{
			grPageActionsBase.DownloadAndCompareGoodsReceiptXmlFromDetailsPage();
		}
	}
}
