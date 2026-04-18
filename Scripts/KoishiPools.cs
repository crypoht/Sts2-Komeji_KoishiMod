using Godot;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Models.CardPools;

namespace KomeijiKoishi.Pools
{
    public class KoishiCardPool : CustomCardPoolModel 
    {
        public override string Title => "koishi"; 
        
        public override string? TextEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_small.png";
        
        public override string? BigEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_big.png";
        
        public override Color DeckEntryCardColor => new(0.486f, 0.988f, 0f);

        public override Color ShaderColor => new(0.486f, 0.988f, 0f);
        
        public override bool IsColorless => true;
    }


    public class KoishiRelicPool : CustomRelicPoolModel 
    {

        public override string? TextEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_small.png";
        
        public override string? BigEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_big.png";
    }


    public class KoishiPotionPool : CustomPotionPoolModel 
    {

        public override string? TextEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_small.png";
        
        public override string? BigEnergyIconPath => "res://mods/Komeiji_Koishi/images/energy_koishi_big.png";
    }
    
    
}