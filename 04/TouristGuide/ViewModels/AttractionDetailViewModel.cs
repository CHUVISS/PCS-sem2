using System.Reactive;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using TouristGuide.Data;
using TouristGuide.Models;

namespace TouristGuide.ViewModels;

public class AttractionDetailViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public AttractionDetailViewModel(MainViewModel main, Attraction attraction)
    {
        _main = main;

        using var db = new AppDbContext();
        Attraction = db.Attractions.Include(a => a.City).FirstOrDefault(a => a.Id == attraction.Id) ?? attraction;

        GoBackCommand = ReactiveCommand.Create(() =>
        {
            if (Attraction.City != null)
                _main.NavigateToCity(Attraction.City);
            else
                _main.NavigateToCities();
        });
    }

    public Attraction Attraction { get; }
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public string TicketPriceFormatted =>
        Attraction.TicketPrice.HasValue
            ? $"{Attraction.TicketPrice.Value:N0} ₽"
            : "Бесплатно";

    public string WorkingHoursFormatted =>
        string.IsNullOrWhiteSpace(Attraction.WorkingHours)
            ? "Не указано"
            : Attraction.WorkingHours;
}
