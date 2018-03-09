﻿using Gtk;
using logicpos.App;
using logicpos.Classes.Enums;
using logicpos.Classes.Enums.Reports;
using logicpos.Classes.Gui.Gtk.Widgets;
using logicpos.datalayer.DataLayer.Xpo;
using logicpos.financial.library.Classes.Finance;
using logicpos.financial.library.Classes.Reports;
using logicpos.resources.Resources.Localization;
using logicpos.shared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace logicpos.Classes.Gui.Gtk.Pos.Dialogs
{
    // Use PrintRouter for financialDocuments ex PrintFinanceDocument

    partial class PosReportsDialog
    {
        // Test Document Report
        void TestDocumentReport()
        {
            Guid docOid = new Guid("6d44b0a8-6450-4245-b4ee-a6e971f4bcec");
            FIN_DocumentFinanceMaster documentFinanceMaster = (FIN_DocumentFinanceMaster)GlobalFramework.SessionXpo.GetObjectByKey(typeof(FIN_DocumentFinanceMaster), docOid);
            if (documentFinanceMaster != null)
            {
                //Generate Default CopyNames from DocumentType
                List<int> copyNames = CustomReport.CopyNames(documentFinanceMaster.DocumentType.PrintCopies);
                string hash4Chars = ProcessFinanceDocument.GenDocumentHash4Chars(documentFinanceMaster.Hash);
                string destinationFileName = "";
                string result = CustomReport.ProcessReportFinanceDocument(CustomReportDisplayMode.Design, documentFinanceMaster.Oid, hash4Chars, copyNames, destinationFileName);
                _log.Debug(String.Format("Result: [{0}]", result));
            }
            else
            {
                _log.Debug(String.Format("Null Document Found for documentType: [{0}]", nameof(FIN_DocumentFinanceMaster), docOid.ToString()));
            }
        }

        void buttonReportUnderConstruction_Clicked(object sender, EventArgs e)
        {
            Utils.ShowMessageUnderConstruction();
        }

        private List<string> GetReportsQueryDialogFilter(ReportsQueryDialogMode pReportsQueryDialogMode, string pDatabaseSourceObject)
        {
            PosReportsQueryDialog dialog = new PosReportsQueryDialog(_sourceWindow, DialogFlags.DestroyWithParent, pReportsQueryDialogMode, pDatabaseSourceObject);
            ResponseType response = (ResponseType)dialog.Run();
            List<string> result = new List<string>();

            if (response == ResponseType.Ok)
            {
                // Assign Dialog FilterValue to Mrthod Result Value
                result.Add(dialog.FilterValue);
                result.Add(dialog.FilterValueHumanReadble);
            }
            else
            {
                // Destroy Dialog on Cancel
                dialog.Destroy();
                // Assign Result
                result = null;
            }

            return result;
        }

        private void PrintReportRouter(object sender, EventArgs e)
        {
            CustomReportDisplayMode displayMode = (Debugger.IsAttached)
                ? CustomReportDisplayMode.Design
                : CustomReportDisplayMode.ExportPDF;
            // Override Default Development Mode
            //displayMode = CustomReportDisplayMode.ExportPDF;

            // Local Variables
            string reportFilter = string.Empty;
            string reportFilterHumanReadable = string.Empty;
            string databaseSourceObject = string.Empty;

            AccordionChildButton button = (sender as AccordionChildButton);
            //_log.Debug(String.Format("Button.Name: [{0}], Button.label: [{1}]", button.Name, button.Label));

            // Get Token From buttonName
            ReportsTypeToken token = (ReportsTypeToken)Enum.Parse(typeof(ReportsTypeToken), button.Name, true);

            // Prepare ReportsQueryDialogMode
            ReportsQueryDialogMode reportsQueryDialogMode = ReportsQueryDialogMode.UNDEFINED;
            // Catch REPORT_SALES_DETAIL_* and REPORT_SALES_DETAIL_GROUP_* use same View
            if (token.ToString().StartsWith("REPORT_SALES_DETAIL_"))
            {
                reportsQueryDialogMode = ReportsQueryDialogMode.FINANCIAL_DETAIL;
                databaseSourceObject = "view_documentfinance";
            }
            else if (token.ToString().StartsWith("REPORT_SALES_"))
            {
                reportsQueryDialogMode = ReportsQueryDialogMode.FINANCIAL;
                databaseSourceObject = "fin_documentfinancemaster";
            }
            else if (token.ToString().Equals("REPORT_LIST_STOCK_MOVEMENTS"))
            {
                reportsQueryDialogMode = ReportsQueryDialogMode.ARTICLE_STOCK_MOVEMENTS;
                databaseSourceObject = "view_articlestock";
            }
            else if (token.ToString().Equals("REPORT_LIST_AUDIT_TABLE"))
            {
                reportsQueryDialogMode = ReportsQueryDialogMode.SYSTEM_AUDIT;
                databaseSourceObject = "view_systemaudit";
            }

            // Common GetReportsQueryDialogFilter for All Non Undefined ReportsQueryDialogMode 
            if (reportsQueryDialogMode != ReportsQueryDialogMode.UNDEFINED)
            {
                // Call PosReportsQueryDialog to get Filter
                List<string> dialogresultFilter = GetReportsQueryDialogFilter(reportsQueryDialogMode, databaseSourceObject);
                if (dialogresultFilter != null && dialogresultFilter.Count == 2)
                {
                    reportFilter = dialogresultFilter[0];
                    reportFilterHumanReadable = dialogresultFilter[1];
                }
                // ResponseType.Cancel
                else
                {
                    reportFilter = null;
                    reportFilterHumanReadable = null;
                }
            }

            // Proceed if we have a Filter != null (ResponseType.Ok), ex can be a string.Empty
            //if (reportFilter != null || !financialViewMode)
            if (reportFilter != null/* || reportsQueryDialogMode != ReportsQueryDialogMode.UNDEFINED*/)
            {
                switch (token)
                {
                    case ReportsTypeToken.REPORT_SALES_PER_FINANCE_DOCUMENT:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.DocumentType.Ord]"
                            , "[DocumentFinanceMaster.DocumentType.Designation] ([DocumentFinanceMaster.DocumentType.Code])",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_DATE:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.DocumentDate]"
                            , "[DocumentFinanceMaster.DocumentDate]",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_USER:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.CreatedBy.Ord]"
                            , "[DocumentFinanceMaster.CreatedBy.Name] ([DocumentFinanceMaster.CreatedBy.Code])",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_TERMINAL:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.CreatedWhere.Ord]"
                            , "[DocumentFinanceMaster.CreatedWhere.Designation] ([DocumentFinanceMaster.CreatedWhere.Code])",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_CUSTOMER:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.EntityFiscalNumber]"
                            , "[DocumentFinanceMaster.EntityName] ([DocumentFinanceMaster.EntityFiscalNumber])",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_PAYMENT_METHOD:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.PaymentCondition.Ord]"
                            , "[DocumentFinanceMaster.PaymentCondition.Designation] ([DocumentFinanceMaster.PaymentCondition.Code])",
                            // Required to Exclude Documents without PaymentMethod else Errors Occur
                            (string.IsNullOrEmpty(reportFilter)) ? "PaymentMethod IS NOT NULL" : string.Format("{0} AND PaymentMethod IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_PAYMENT_CONDITION:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.PaymentCondition.Ord]"
                            , "[DocumentFinanceMaster.PaymentCondition.Designation] ([DocumentFinanceMaster.PaymentCondition.Code])",
                            // Required to Exclude Documents without PaymentCondition else Errors Occur
                            (string.IsNullOrEmpty(reportFilter)) ? "PaymentCondition IS NOT NULL" : string.Format("{0} AND PaymentCondition IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_CURRENCY:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.Currency.Ord]"
                            , "[DocumentFinanceMaster.Currency.Designation] ([DocumentFinanceMaster.Currency.Code])",
                            // Required to Exclude Documents without PaymentCondition else Errors Occur
                            (string.IsNullOrEmpty(reportFilter)) ? "PaymentCondition IS NOT NULL" : string.Format("{0} AND PaymentCondition IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_PER_COUNTRY:
                        CustomReport.ProcessReportDocumentMasterList(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceMaster.EntityCountry]"
                            , "[DocumentFinanceMaster.EntityCountry]",
                            reportFilter,
                            reportFilterHumanReadable
                            );
                        break;

                    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_FINANCE_DOCUMENT:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_FINANCE_DOCUMENT:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "ftOid, fdArticle"
                            , "[DocumentFinanceDetail.DocumentTypeOrd]"
                            , "[DocumentFinanceDetail.DocumentTypeDesignation] ([DocumentFinanceDetail.DocumentTypeCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_DATE:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_DATE:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "fmDocumentDate, fdArticle"
                            , "[DocumentFinanceDetail.DocumentDate]"
                            , "[DocumentFinanceDetail.DocumentDate]",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_USER:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_USER:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "udUserDetail, fdArticle"
                            , "[DocumentFinanceDetail.UserDetailOrd]"
                            , "[DocumentFinanceDetail.UserDetailName] ([DocumentFinanceDetail.UserDetailCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_TERMINAL:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_TERMINAL:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "trTerminal, fdArticle"

                            , "[DocumentFinanceDetail.TerminalOrd]"
                            , "[DocumentFinanceDetail.TerminalDesignation] ([DocumentFinanceDetail.TerminalCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_CUSTOMER:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_CUSTOMER:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "fmEntityFiscalNumber, fdArticle"
                            , "[DocumentFinanceDetail.EntityFiscalNumber]"
                            , "[DocumentFinanceDetail.EntityName] ([DocumentFinanceDetail.EntityFiscalNumber])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_PAYMENT_METHOD:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_PAYMENT_METHOD:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "fmPaymentMethod, fdArticle"
                            , "[DocumentFinanceDetail.PaymentMethodOrd]"
                            , "[DocumentFinanceDetail.PaymentMethodDesignation] ([DocumentFinanceDetail.PaymentMethodCode])",
                            (string.IsNullOrEmpty(reportFilter)) ? "fmPaymentMethod IS NOT NULL" : string.Format("{0} AND fmPaymentMethod IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_PAYMENT_CONDITION:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_PAYMENT_CONDITION:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "fmPaymentCondition, fdArticle"
                            , "[DocumentFinanceDetail.PaymentConditionOrd]"
                            , "[DocumentFinanceDetail.PaymentConditionDesignation] ([DocumentFinanceDetail.PaymentConditionCode])",
                            (string.IsNullOrEmpty(reportFilter)) ? "fmPaymentCondition IS NOT NULL" : string.Format("{0} AND fmPaymentCondition IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_CURRENCY:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_CURRENCY:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "fmCurrency, fdArticle"
                            , "[DocumentFinanceDetail.CurrencyOrd]"
                            , "[DocumentFinanceDetail.CurrencyDesignation] ([DocumentFinanceDetail.CurrencyCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_COUNTRY:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_COUNTRY:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "ccCountry, fdArticle"
                            , "[DocumentFinanceDetail.CountryOrd]"
                            , "[DocumentFinanceDetail.CountryDesignation] ([DocumentFinanceDetail.EntityCountryCode2])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_FAMILY:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_FAMILY:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "afFamily, fdArticle"
                            , "[DocumentFinanceDetail.ArticleFamilyOrd]"
                            , "[DocumentFinanceDetail.ArticleFamilyDesignation] ([DocumentFinanceDetail.ArticleFamilyCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_FAMILY_AND_SUBFAMILY:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_FAMILY_AND_SUBFAMILY:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "sfSubFamily, fdArticle"
                            , "[DocumentFinanceDetail.ArticleSubFamilyOrd]"
                            , "[DocumentFinanceDetail.ArticleFamilyDesignation] ([DocumentFinanceDetail.ArticleFamilyCode]) / [DocumentFinanceDetail.ArticleSubFamilyDesignation] ([DocumentFinanceDetail.ArticleSubFamilyCode])",
                            reportFilter,
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_PLACE:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_PLACE:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "cpPlace, fdArticle"
                            , "[DocumentFinanceDetail.PlaceOrd]"
                            , "[DocumentFinanceDetail.PlaceDesignation] ([DocumentFinanceDetail.PlaceCode])",
                            // Required to Exclude Documents without Place
                            (string.IsNullOrEmpty(reportFilter)) ? "cpPlace IS NOT NULL" : string.Format("{0} AND cpPlace IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;
                    case ReportsTypeToken.REPORT_SALES_DETAIL_PER_PLACE_TABLE:
                    case ReportsTypeToken.REPORT_SALES_DETAIL_GROUP_PER_PLACE_TABLE:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , token.ToString().ToLower()
, "dmPlaceTable, fdArticle"
                            , "[DocumentFinanceDetail.PlaceTableOrd]"
                            , "[DocumentFinanceDetail.PlaceDesignation] ([DocumentFinanceDetail.PlaceCode]) / [DocumentFinanceDetail.PlaceTableDesignation] ([DocumentFinanceDetail.PlaceTableCode])",
                            // Required to Exclude Documents without PlaceTable
                            (string.IsNullOrEmpty(reportFilter)) ? "dmPlaceTable IS NOT NULL" : string.Format("{0} AND dmPlaceTable IS NOT NULL", reportFilter),
                            reportFilterHumanReadable
                            , (token.ToString().Contains("_DETAIL_GROUP_"))
                            );
                        break;

                    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                    // Where it is Called?
                    /*
                    case ReportsTypeToken.REPORT_SALES_PER_FAMILY_AND_SUBFAMILY:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceDetail.ArticleFamilyCode]"
                            , "[DocumentFinanceDetail.ArticleFamilyDesignation] ([DocumentFinanceDetail.ArticleFamilyCode])"
                            , false
                            );
                        break;
                    // Where it is Called?
                    case ReportsTypeToken.REPORT_SALES_PER_ZONE_TABLE:
                        CustomReport.ProcessReportDocumentDetail(displayMode
                            , Resx.ResourceManager.GetString(token.ToString().ToLower())
                            , "[DocumentFinanceDetail.ArticleFamilyCode]"
                            , "[DocumentFinanceDetail.ArticleFamilyDesignation] ([DocumentFinanceDetail.ArticleFamilyCode])"
                            , true
                            );
                        break;
                    */

                    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                    case ReportsTypeToken.REPORT_TOTAL_PER_FAMILY:
                        break;
                    case ReportsTypeToken.REPORT_TOP_CLOSE_EMPLOYEES:
                        break;
                    case ReportsTypeToken.REPORT_OCCUPATION_AVERAGE:
                        break;
                    case ReportsTypeToken.REPORT_ZONE_TOTAL:
                        break;
                    case ReportsTypeToken.REPORT_CLOSE_PEAK_HOUR:
                        break;
                    case ReportsTypeToken.REPORT_TOP_OFFERS:
                        break;
                    case ReportsTypeToken.REPORT_TOP_EMPLOYEE_RECORDS:
                        break;
                    case ReportsTypeToken.REPORT_RECORD_PEAK_HOUR:
                        break;
                    case ReportsTypeToken.REPORT_EMPLOYEE_MOVENTS:
                        break;
                    case ReportsTypeToken.REPORT_ACOUNT_BALANCE:
                        break;
                    case ReportsTypeToken.REPORT_WITHHOLDING_TAX:
                        break;
                    case ReportsTypeToken.REPORT_BALANCE_SHEET:
                        break;
                    case ReportsTypeToken.REPORT_SERVICE_HOURS:
                        break;
                    case ReportsTypeToken.REPORT_COURIER_DELIVER:
                        break;
                    case ReportsTypeToken.REPORT_CANCELED_ARTICLES_PER_EMPLOYEE:
                        break;
                    case ReportsTypeToken.REPORT_LIST_INVENTORY:
                        break;
                    case ReportsTypeToken.REPORT_DISCOUNTS_PER_USER:
                        break;
                    case ReportsTypeToken.REPORT_LIST_CONSUMPTION_PER_USER:
                        break;
                    case ReportsTypeToken.REPORT_CASH_TOTAL:
                        break;
                    case ReportsTypeToken.REPORT_LIST_WORKSESSION:
                        break;
                    case ReportsTypeToken.REPORT_LIST_CUSTOMERS:
                        CustomReport.ProcessReportCustomer(displayMode);
                        break;
                    case ReportsTypeToken.REPORT_LIST_CLOSE_WORKSESSION:
                        break;
                    case ReportsTypeToken.REPORT_LIST_FAMILY_SUBFAMILY_ARTICLES:
                        // Where it is Called?
                        CustomReport.ProcessReportArticle(displayMode);
                        break;
                    case ReportsTypeToken.REPORT_LIST_STOCK_MOVEMENTS:
                        CustomReport.ProcessReportArticleStockMovement(displayMode, reportFilter, reportFilterHumanReadable);
                        break;
                    case ReportsTypeToken.REPORT_LIST_AUDIT_TABLE:
                        CustomReport.ProcessReportSystemAudit(displayMode, reportFilter, reportFilterHumanReadable);
                        break;
                    default:
                        _log.Error(String.Format("Undetected Token: [{0}]", token));
                        break;
                }
            }
        }
    }
}