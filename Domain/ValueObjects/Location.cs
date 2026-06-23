using Domain.Exceptions;
namespace Domain.ValueObjects;

public readonly record struct Location
{
    public double Latitude{get; init;}
    public double Longitude{get; init;}

    private Location(double latitude, double longitude)
    {
        if(latitude < -90 || latitude > 90)
            throw new DomainException($"latitude {latitude} not in -90 to 90");

        if(longitude < -180 || longitude > 180)
            throw new DomainException($"longitude {longitude} not in -180 to 180");

        Latitude = Math.Round(latitude,6);
        Longitude = Math.Round(longitude,6);
    }

    public static Location FromCoordinates(double latitude, double longitude) => new(latitude,longitude);


    public double DistanceTo(Location other)
    {
        const double R = 6371;
        var lat1 = Latitude * Math.PI / 180;
        var lat2 = other.Latitude * Math.PI /180;
        var dLat = (other.Latitude - Latitude) * Math.PI / 180;
        var dLon = (other.Longitude - Longitude) * Math.PI / 180;

        var a = Math.Sin(dLat /2) * Math.Sin(dLat/2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon /2) * Math.Sin(dLon /2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a),Math.Sqrt(1-a));
        return R *c;
    }
}