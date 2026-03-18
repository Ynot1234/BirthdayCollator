// 1. External Libraries
global using HtmlAgilityPack;

// 2. Core Models & Constants
global using BirthdayCollator.Server.Models;
global using BirthdayCollator.Server.Constants;
global using static BirthdayCollator.Server.Constants.AppStrings;

// 3. Infrastructure & Networking
global using BirthdayCollator.Server.Configuration;  
global using BirthdayCollator.Server.Processing.Fetching;
global using System.Globalization;
global using static System.Globalization.CultureInfo;

// 4. Processing & Logic
global using BirthdayCollator.Server.Helpers;
global using BirthdayCollator.Server.Processing.Html;
global using BirthdayCollator.Server.Processing.Links;
global using BirthdayCollator.Server.Processing.Builders;
global using BirthdayCollator.Server.Processing.Pipelines;
global using BirthdayCollator.Server.Processing.Parsers;
global using BirthdayCollator.Server.Processing.Enrichment;
global using BirthdayCollator.Server.Processing.Validation;
global using BirthdayCollator.Server.Processing.Sources;