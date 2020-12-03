﻿using SwedbankPay.Sdk.PaymentInstruments.Card;
using System;
using System.Collections.Generic;

namespace SwedbankPay.Sdk.PaymentInstruments
{
    internal class ReversalsListResponseDto
    {
        public string Id { get; set; }
        public List<TransactionDto> ReversalList { get; set; } = new List<TransactionDto>();
        internal IReversalsListResponse Map()
        {
            var list = new List<ITransactionResponse>();
            foreach (var item in ReversalList)
            {
                list.Add(new TransactionResponse(Id, item));
            }

            Uri id = new Uri(Id, UriKind.RelativeOrAbsolute);
            return new ReversalsListResponse(id, list);
        }
    }
}