﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atata;
using NUnit.Framework;
using Sample.AspNetCore.SystemTests.Test.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Exceptions;
using SwedbankPay.Sdk.PaymentInstruments;

namespace Sample.AspNetCore.SystemTests.Test.PaymentTests.Payment
{
    public class PaymentReversalTests : Base.PaymentTests
    {
        public PaymentReversalTests(string driverAlias)
            : base(driverAlias)
        {
        }


        [Test]
        [Retry(2)]
        [TestCaseSource(nameof(TestData), new object[] { false, PaymentMethods.Card })]
        public async Task Payment_Card_Reversal(Product[] products, PayexInfo payexInfo)
        {
	        GoToOrdersPage(products, payexInfo, Checkout.Option.LocalPaymentMenu)
                .RefreshPageUntil(x => x.Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateCapture)].IsVisible, 60, 10)
                .Orders[y => y.Content.Value.Contains(_referenceLink)].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateCapture)].ExecuteAction.ClickAndGo()
                .RefreshPageUntil(x => x.Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateReversal)].IsVisible, 60, 10)
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateReversal)].ExecuteAction.ClickAndGo()
                .RefreshPageUntil(x => x.Orders[y => y.Content.Value.Contains(_referenceLink)].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.PaidPayment)].IsVisible, 60, 10)
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.PaidPayment)].Should.BeVisible()
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.ViewPayment)].Should.BeVisible()
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Clear.ClickAndGo();

            var order = await SwedbankPayClient.Payments.CardPayments.Get(new Uri(_referenceLink, UriKind.RelativeOrAbsolute), PaymentExpand.All);

            // Operations
            Assert.That(order.Operations[LinkRelation.CreateCancellation], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreateCapture], Is.Null);
            Assert.That(order.Operations[LinkRelation.CreateReversal], Is.Null);
            Assert.That(order.Operations[LinkRelation.PaidPayment], Is.Not.Null);

            // Transactions
            Assert.That(order.Payment.Transactions.TransactionList.Count, Is.EqualTo(3));
            Assert.That(order.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Authorization).State,
                        Is.EqualTo(State.Completed));
            Assert.That(order.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Capture).State,
                        Is.EqualTo(State.Completed));
            Assert.That(order.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Reversal).State,
                        Is.EqualTo(State.Completed));
        }


        [Test]
        [Retry(9)] //Retry several times because of the untreatable error: "Activity chain is broken."
        [TestCaseSource(nameof(TestData), new object[] { false, PaymentMethods.Swish })]
        public async Task Payment_Swish_Reversal(Product[] products, PayexInfo payexInfo)
        {
	        GoToOrdersPage(products, payexInfo, Checkout.Option.LocalPaymentMenu)
                .RefreshPageUntil(x => x.Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateReversal)].IsVisible, 60, 10)
                .Orders[y => y.Content.Value.Contains(_referenceLink)].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.CreateReversal)].ExecuteAction.ClickAndGo()
                .RefreshPageUntil(x => x.Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.ViewPayment)].IsVisible, 60, 10)
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Actions.Rows[y => y.Name.Value.Contains(PaymentResourceOperations.ViewPayment)].Should.BeVisible()
                .Orders[y => y.Attributes["data-paymentlink"] == _referenceLink].Clear.ClickAndGo();

            var swishPayment = await SwedbankPayClient.Payments.SwishPayments.Get(new Uri(_referenceLink, UriKind.RelativeOrAbsolute), PaymentExpand.All);
            var counter = 0;

            while (swishPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Reversal).State != State.Completed && counter <= 15)
            {
                Thread.Sleep(1000);
                try
                {
                    swishPayment = await SwedbankPayClient.Payments.SwishPayments.Get(_link, PaymentExpand.All);
                }
                catch (HttpResponseException){}

                counter++;
            }

            // Operations
            Assert.That(swishPayment.Operations[LinkRelation.CreateCancellation], Is.Null);
            Assert.That(swishPayment.Operations[LinkRelation.CreateCapture], Is.Null);
            Assert.That(swishPayment.Operations[LinkRelation.CreateReversal], Is.Null);
            Assert.That(swishPayment.Operations[LinkRelation.ViewPayment], Is.Not.Null);

            // Transactions
            Assert.That(swishPayment.Payment.Transactions.TransactionList.Count, Is.EqualTo(2));
            Assert.That(swishPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Sale).State,
                        Is.EqualTo(State.Completed));
            Assert.That(swishPayment.Payment.Transactions.TransactionList.First(x => x.Type == TransactionType.Reversal).State,
                        Is.EqualTo(State.Completed));
        }

    }
}
