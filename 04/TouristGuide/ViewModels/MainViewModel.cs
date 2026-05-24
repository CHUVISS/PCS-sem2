using System.Reactive;
using ReactiveUI;
using TouristGuide.Models;

namespace TouristGuide.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentView = null!;

    public MainViewModel()
    {
        var citiesVm = new CitiesViewModel(this);
        CurrentView = citiesVm;
        NavigateToCitiesCommand = ReactiveCommand.Create(NavigateToCities);
    }

    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public ReactiveCommand<Unit, Unit> NavigateToCitiesCommand { get; }

    public void NavigateToCities()
    {
        CurrentView = new CitiesViewModel(this);
    }

    public void NavigateToCity(City city)
    {
        CurrentView = new CityDetailViewModel(this, city);
    }

    public void NavigateToAttraction(Attraction attraction)
    {
        CurrentView = new AttractionDetailViewModel(this, attraction);
    }
}
