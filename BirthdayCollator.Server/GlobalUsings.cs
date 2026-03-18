// 1. External Libraries
global using HtmlAgilityPack;

// 2. Core Models & Constants
global using BirthdayCollator.Server.Models;
global using BirthdayCollator.Server.Constants;
global using static BirthdayCollator.Server.Constants.AppStrings;

// 3. Infrastructure & Networking
global using BirthdayCollator.Server.Configuration;  
global using BirthdayCollator.Server.Processing.Fetching;

// 4. Processing & Logic
global using BirthdayCollator.Server.Helpers;
global using BirthdayCollator.Server.Processing.Html;
global using BirthdayCollator.Server.Processing.Links;
global using BirthdayCollator.Server.Processing.Builders;
global using BirthdayCollator.Server.Processing.Pipelines; 