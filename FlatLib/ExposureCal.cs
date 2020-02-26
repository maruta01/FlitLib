using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlatLib
{
    public class ExposuerCal
    {
 
            private string filterName; // Filter Name
            private double lastAdu; // Lasted Adu 
            private double lastExposureTime; // Lasted Exposure Time
            private double targetAdu; // Adu Target 
            private int grabTime; // grab at time  ** start at 1 **
            private double exposureTimeCalculate; // calculated exposure Time  for grab 
            private double variableAdu; // use for calculate next grab
            private static MongoClient client = new MongoClient();

            public string databaseName = "CCDFlat";




            public double flatExposureTime(string filterName, double lastAdu, double lastExposureTime, double targetAdu, int grabTime)
            {
                this.filterName = filterName;
                this.lastAdu = lastAdu;
                this.lastExposureTime = lastExposureTime;
                this.targetAdu = targetAdu;
                this.grabTime = grabTime;

                if (this.grabTime == 1) // frist grab for use frist Adu referent  .....
                {
                    this.variableAdu = this.lastAdu;
                }
                double aduCostError = this.lastAdu - this.targetAdu; // find difference between lastAdu and adu Target ..

                if (aduCostError < 0) // LastADU less than TargetADU..
                {
                    this.variableAdu = this.variableAdu + Math.Abs(aduCostError);
                }
                else if (aduCostError > 0) // LastADU more than TargetADU..
                {
                    this.variableAdu = variableAdu - Math.Abs(aduCostError);
                }
                exposureTimeCalculate = exposureCal(this.lastExposureTime, this.lastAdu, variableAdu); //Calculate the next exposure Time ..

                return (this.exposureTimeCalculate);
            }

            private double exposureCal(double oldExposure, double adu, double variableAdu)
            {
                double simulete = adu / oldExposure;
                double exposureCal = variableAdu / simulete;
                return (exposureCal);
            }

            public double exposureTimeFristGrab(double sunAlt, string filter, double targetAdu)
            {
                double fristExposureTime = 0;
                try
                {
                    IMongoDatabase db = client.GetDatabase(this.databaseName);
                    var things = db.GetCollection<CCD_Mongo>("data");
                    var sunaltDB = things.AsQueryable().Where(x => x.SunAltStart >= (sunAlt - 0.2) & x.SunAltStart < (sunAlt + 0.2) & x.Filter == filter & x.Adu >= (targetAdu - 1000) & x.Adu <= (targetAdu + 1000)).Max(x => x.SunAltStart);
                    var command = things.AsQueryable().Where(y => y.SunAltStart == sunaltDB & y.Filter == filter).FirstOrDefault();
                    fristExposureTime = command.ExposureTime;
                }
                catch
                {
                    fristExposureTime = 1.00;
                }
                return (fristExposureTime);

            }
        }
    }


internal class CCD_Mongo
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("SunAltStart")]
    public double SunAltStart { get; set; }

    [BsonElement("SunAltEnd")]
    public double SunAltEnd { get; set; }

    [BsonElement("SunAzmStart")]
    public double SunAzmStart { get; set; }
    [BsonElement("SunAzmEnd")]
    public double SunAzmEnd { get; set; }

    [BsonElement("Filter")]
    public string Filter { get; set; }

    [BsonElement("ExposureTime")]
    public double ExposureTime { get; set; }

    [BsonElement("Adu")]
    public double Adu { get; set; }

    [BsonElement("FitsImage")]
    public string FitsImage { get; set; }

    [BsonElement("JpgImage")]
    public string JpgImage { get; set; }

    [BsonElement("AllskyImage")]
    public string AllskyImage { get; set; }

    [BsonElement("DateTime")]
    public DateTime DateTime { get; set; }
}

