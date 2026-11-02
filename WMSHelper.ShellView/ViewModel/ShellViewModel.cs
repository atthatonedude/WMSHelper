using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WMSHelper.ShellView.Interface;
using WMSHelper.ShellView.Model;

namespace WMSHelper.ShellView.ViewModel
{
    public class ShellViewModel : INotifyPropertyChanged
    {

        private readonly SecondWindow secondWindow = new SecondWindow();
        private readonly ThirdWindow thirdWindow = new ThirdWindow();
        public ObservableCollection<PrintItemModel> PrintList { get; } = new();

        //private List<PrintItemModel> printList;


        //this converts bool to visibility for the print column. Visibility prop is enum not bool so we need to convert it.
        private bool isVisible = false;






        private object? _currentView = new SecondWindow();
        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand OpenSecondWindowCommand { get; }
        public ICommand OpenThirdWindowCommand { get; }

        public ICommand TogglePrintColumnCommand { get; }

        public ICommand AddtoListCommand { get; }




        public ShellViewModel()
        {
            
            OpenSecondWindowCommand = new RelayCommand(_ => CurrentView = secondWindow);
            OpenThirdWindowCommand = new RelayCommand(_ => CurrentView = thirdWindow);
            TogglePrintColumnCommand = new RelayCommand(_ => IsVisible = !IsVisible);
            AddtoListCommand = new RelayCommand(
            param => AddToList(param), param => true);

            PopulatePrintList();

        }



        public void PopulatePrintList()
        {
            PrintList.Add(new PrintItemModel("Printer 1"));
            PrintList.Add(new PrintItemModel("Printer 2"));
            PrintList.Add(new PrintItemModel("Printer 3"));
        }
        private  void AddToList(object? param)
        {
            PrintList.Add(new PrintItemModel(param.ToString()));
            


        }


        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



    }
}
