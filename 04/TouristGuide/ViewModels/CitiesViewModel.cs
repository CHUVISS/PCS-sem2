using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using TouristGuide.Data;
using TouristGuide.Models;

namespace TouristGuide.ViewModels;

public class CitiesViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _searchText = string.Empty;
    private ObservableCollection<City> _cities = new();

    public CitiesViewModel(MainViewModel main)
    {
        _main = main;
        LoadCities();

        SelectCityCommand = ReactiveCommand.Create<City>(city => _main.NavigateToCity(city));
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterCities();
        }
    }

    public ObservableCollection<City> Cities
    {
        get => _cities;
        set => this.RaiseAndSetIfChanged(ref _cities, value);
    }

    public ReactiveCommand<City, Unit> SelectCityCommand { get; }

    private List<City> _allCities = new();

    private void LoadCities()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        _allCities = db.Cities.Include(c => c.Attractions).ToList();
        Cities = new ObservableCollection<City>(_allCities);
    }

    private void FilterCities()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Cities = new ObservableCollection<City>(_allCities);
        }
        else
        {
            var filtered = _allCities
                .Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            c.Region.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Cities = new ObservableCollection<City>(filtered);
        }
    }
}
