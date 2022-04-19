﻿using MonobankExporter.API.Models;

namespace MonobankExporter.API.Interfaces
{
    public interface IPrometheusExporterService
    {
        public void ObserveAccount(AccountInfoModel account, double balance);
        public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
    }
}