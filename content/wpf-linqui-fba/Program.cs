#:sdk Microsoft.NET.Sdk

#:package CommunityToolkit.Mvvm@8.4.0
#:package LinqUI.WPF@1.0.6

#:property OutputType=WinExe
#:property TargetFramework=NET_TFM_WINDOWS
#:property UseWPF=True
#:property UseWindowsForms=False

// WPF cannot use AOT compilation.
#:property PublishAot=False

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinqUI.WPF;

// https://github.com/dotnet/winforms/issues/5071#issuecomment-908789632
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

new Application()
        .ShutdownMode(ShutdownMode.OnMainWindowClose)
        .OnDispatcherUnhandledException((_, e) =>
        {
            MessageBox.Show(e.Exception.ToString(), "Unhandled", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        })
        .Run(new Window()
                .WindowStartupLocation(WindowStartupLocation.CenterScreen)
                .Title($"Hello, World! - {Thread.CurrentThread.GetApartmentState()}")
                .Size(640, 240)
                .DataContext(new CounterViewModel())
                .Content(
                    new StackPanel()
                        .Children(
                            new TextBlock()
                                .FontSize(24)
                                .Margin(8)
                                .HCenter()
                                .Bind(TextBlock.TextProperty, new Binding(nameof(CounterViewModel.Count))),
                            new Button()
                                .Content("Increment (+1)")
                                .Margin(8)
                                .Bind(Button.CommandProperty, new Binding(nameof(CounterViewModel.IncrementCountCommand))),
                            new Button()
                                .Content("Decrement (-1)")
                                .Margin(8)
                                .Bind(Button.CommandProperty, new Binding(nameof(CounterViewModel.DecrementCountCommand)))
                        )
                )
    );

public sealed partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count = 0;

    [RelayCommand]
    private void IncrementCount()
        => Count++;

    [RelayCommand]
    private void DecrementCount()
        => Count--;
}
