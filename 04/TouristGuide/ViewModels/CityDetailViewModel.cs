using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using TouristGuide.Data;
using TouristGuide.Models;

namespace TouristGuide.ViewModels;

public class CityDetailViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public CityDetailViewModel(MainViewModel main, City city)
    {
        _main = main;

        // Load full city with attractions from DB
        using var db = new AppDbContext();
        City = db.Cities.Include(c => c.Attractions).FirstOrDefault(c => c.Id == city.Id) ?? city;
        Attractions = new ObservableCollection<Attraction>(City.Attractions);

        GoBackCommand = ReactiveCommand.Create(() => _main.NavigateToCities());
        SelectAttractionCommand = ReactiveCommand.Create<Attraction>(a => _main.NavigateToAttraction(a));
    }

    public City City { get; }
    public ObservableCollection<Attraction> Attractions { get; }
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Attraction, Unit> SelectAttractionCommand { get; }

    public string PopulationFormatted => $"{City.Population:N0} чел.";
}
