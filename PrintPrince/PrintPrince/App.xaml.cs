using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;

namespace PrintPrince
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void OnStartup(StartupEventArgs e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            SimpleIoc.Default.Register<IDialogService>(() => new DialogService());
        }
    }
}
