namespace SmartFinance.Domain.Enums;

public enum OfferType
{
    Insight = 0,       // "işte paran nereye gidiyor" — ayna/içgörü
    Saving = 1,        // "şuradan tasarruf edebilirsin"
    Campaign = 2,      // "sana özel kampanya"
    Trend = 3          // "geçen aya göre değişim" (zaman gerektirir)
}