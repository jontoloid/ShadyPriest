using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZzukBot.Engines.CustomClass;
using ZzukBot.Engines.CustomClass.Objects;

/*

CREDITS
to krycess for his CasinoFury2, I have taken his approach to handling adds and selecting drinks and implemented those in here.
to uh.. Emu?
to Fedelis for his FedLock - it seems his approach of GCD checking via CanUse has fixed my wanding issue.


05000000000000000000000000000005002520103511051
*/

namespace ShadyForm
{
    internal static class Constants 
    {
        public static readonly Version Release = new Version(0, 1);

        /* http://rpgworld.altervista.org/classic_vanilla_talent/priest.php */
        public static readonly string[] TalentStrings = 
        {
            "05000000000000000000000000000005002520103511051"
        };
        
    }

    public class ShadyForm : CustomClass
    {
        private readonly TalentManager talentManager;
        public ShadyForm() 
        {
            this.talentManager = new TalentManager(this.Player);
        }

        public override byte DesignedForClass
        {
            get
            {
                return PlayerClass.Priest;
            }
        }

        public override string CustomClassName
        {
            get
            {
                return "Shadyform 1.2.0";
            }
        }

		/*
		Change this value to determine the point at which you want to stop using mind flay and start wanding.
        for example: 65 for mind flay until the enemy has 65% health, wanding from then on.
        I feel like this is highly subjective and depends on how much time you want to spend drinking / how good your wand damage is,
        so play around with it to figure something out that suits your character.
        Change useShadowForm to true in case you have specced into it.
        Change useSilence to true in case you have specced into it / want to use it.
        useWand should never have to be changed, this was kind of experimental. The no-wand logic will still trigger with this set to true.
        Do NOT set debug to true unless you want your ingame chat to be spammed with status messages. I use this to figure out confilcting conditions.
        (Like why on earth the character starts trying to spam wand before the DoTs are even finished applying)
        */
        bool isPlayerSub14 = false;
        int healthP = 65;
        bool useSilence = false;
        bool useShadowForm = true;
        bool debug = false;
        bool useVEmb = false;
        bool useWand = true;
        bool useTouchOfWeakness = false;
        bool useMultiDot = true;
        



         public string[] drinkNames = {"Refreshing Spring Water", "Ice Cold Milk",
            "Melon Juice", "Moonberry Juice",
            "Sweet Nectar", "Morning Glory Dew", "Conjured Purified Water", "Conjured Spring Water", "Conjured Mineral Water", "Conjured Sparkling Water", "Conjured Crystal Water" };


       	public string[] nastyDiseases = {};

       	public string[] nastyDebuffs = {"Soul Siphon", };

       
       /*public int playerLevel()
       {
        int playerLevel = Convert.ToInt32(this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage(UnitLevel('player'))"));
        return playerLevel;
       }*/

       public void dispelDiseases()
       {

       }

       public void dispelMagic()
       {

       }

       public void pullPriority()
       {
        // string playerLevel = this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage(UnitLevel('player'))");
        // playerLevel;
            if (this.Player.GetSpellRank("Mind Blast") != 0)
            {
                if (this.Player.CanUse("Mind Blast"))
                {
                    this.Player.Cast("Mind Blast");
                }
            }
            else if (this.Player.GetSpellRank("Shadow Word: Pain") != 0)
            {
                if (!Target.GotDebuff("Shadow Word: Pain"))
                {
                    this.Player.Cast("Shadow Word: Pain");
                }
            }
            else
            {	// to ensure this works from level 1.
                this.Player.Cast("Smite");
            }
		}


        public bool MultiDotting()
        {
            if (this.Attackers.Count >= 2 && this.Player.ManaPercent >= 40 && useMultiDot == true)
            {
                var properTarget = this.Attackers.FirstOrDefault(t => t.HealthPercent <= 99);
                int newAddH = this.Attackers.Max(Mob => Mob.HealthPercent);
                var newAdd = this.Attackers.SingleOrDefault(Mob => Mob.HealthPercent == newAddH);
                if (newAdd != null && newAdd.Guid != this.Target.Guid && !newAdd.GotDebuff("Shadow Word: Pain"))
                {
                    if (this.Player.GetSpellRank("Shadow Word: Pain") != 0)
                    {
                        if (this.Player.CanUse("Shadow Word: Pain"))
                        {
                            this.Player.SetTargetTo(newAdd);
                            this.Player.Cast("Shadow Word: Pain");
                        }
                    }
                }
                this.Player.SetTargetTo(properTarget);
            }
            return true;
        }

        public void gotWand()
        {
        
	        if (useShadowForm)
	        {
				if (this.Player.ManaPercent < 10 || this.Target.HealthPercent < healthP)
	        	{
	            	if (this.Player.IsChanneling != "Mind Flay" && this.Player.IsCasting != "Mind Flay")
	            	{
	            		if(debug==true)
	            		{
	            			this.Player.DoString("OutPut1 = 'Starting to Wand'");
	            			this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage('trying to start wanding with shadowform')");
	            		}
	                	this.Player.StartWand();
	            	}
	           		
	            }
	        } 
	        if(debug==true)
	            {
	            	this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage('trying to start wanding without SForm')");
	            }
			this.Player.StartWand();
		}

	    public void noWand()
	    {
	    	if(debug==true)
	            {
	            	this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage('no wand present! attacking otherwise')");
	            }
	    	if (this.Player.ManaPercent > 50)
	    	{
	    		this.Player.Cast("Smite");
	    	}
	    	else if (this.Target.IsFleeing)
	    	{
	    		this.Player.Cast("Smite");
	    	}
	    	else
	    	{
	    		this.Player.Attack();
	    	}
	    }


	    public void UseScrolls()
	    {

	    }

        public void SelectDrink()
        {
            if (this.Player.ItemCount("Morning Glory Dew") != 0)
                this.Player.Drink(drinkNames[5]);
            else if (this.Player.ItemCount("Sweet Nectar") != 0)
                this.Player.Drink(drinkNames[4]);
            else if (this.Player.ItemCount("Moonberry Juice") != 0)
                this.Player.Drink(drinkNames[3]);
            else if (this.Player.ItemCount("Melon Juice") != 0)
                this.Player.Drink(drinkNames[2]);
            else if (this.Player.ItemCount("Ice Cold Milk") != 0)
                this.Player.Drink(drinkNames[1]);
            else if (this.Player.ItemCount("Refreshing Spring Water") != 0)
                this.Player.Drink(drinkNames[0]);
            else if (this.Player.ItemCount("Conjured Purified Water") != 0)
                this.Player.Drink(drinkNames[6]);
            else if (this.Player.ItemCount("Conjured Spring Water") != 0)
                this.Player.Drink(drinkNames[7]);
            else if (this.Player.ItemCount("Conjured Mineral Water") != 0)
                this.Player.Drink(drinkNames[8]);
            else if (this.Player.ItemCount("Conjured Sparkling Water") != 0)
                this.Player.Drink(drinkNames[9]);
            else if (this.Player.ItemCount("Conjured Crystal Water") != 0)
                this.Player.Drink(drinkNames[10]);        
            }


        public void SelectHPotion()
        {
            if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Major Healing Potion") != 0)
                this.Player.UseItem("Major Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Superior Healing Potion") != 0)
                this.Player.UseItem("Superior Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Greater Healing Potion") != 0)
                this.Player.UseItem("Greater Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Healing Potion") != 0)
                this.Player.UseItem("Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Discolored Healing Potion") != 0)
                this.Player.UseItem("Discolored Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Lesser Healing Potion") != 0)
                this.Player.UseItem("Lesser Healing Potion");
            else if (this.Player.HealthPercent <= 20 && this.Player.ItemCount("Minor Healing Potion") != 0)
                this.Player.UseItem("Minor Healing Potion");
        }


        public void SelectMPotion()
        {
            if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Major Mana Potion") != 0)
                this.Player.UseItem("Major Mana Potion");
            else if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Superior Mana Potion") != 0)
                this.Player.UseItem("Superior Mana Potion");
            else if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Greater Mana Potion") != 0)
                this.Player.UseItem("Greater Mana Potion");
            else if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Mana Potion") != 0)
                this.Player.UseItem("Mana Potion");
            else if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Lesser Mana Potion") != 0)
                this.Player.UseItem("Lesser Mana Potion");
            else if (this.Player.ManaPercent <= 20 && this.Player.ItemCount("Minor Healing Potion") != 0)
                this.Player.UseItem("Minor Mana Potion");
        }

        public void SilenceEnemy()
        {   //I will be using this condition until I can figure out why Silence is sometimes used at random.
        	if (this.Player.ManaPercent >= 75)
        	{	//lookig for spells being cast right now
	            if (this.Target.IsCasting != "" || this.Target.IsChanneling != "")
	            {	// checking, if you are specced into Silence
	                if (this.Player.GetSpellRank("Silence") != 0)
	                {	//Off CD?
	                    if (this.Player.CanUse("Silence"))
	                    {
	                        this.Player.StopCasting();
	                        this.Player.Cast("Silence");
	                        return;
	                    }
	                }
	            }
	        }
    	}

        public void MultipleEnemies()
        {
           if (this.Player.GetSpellRank("Psychic Scream") != 0 && this.Attackers.Count >= 2 && this.Player.CanUse("Psychic Scream") && this.Player.ManaPercent >= 30 && this.Player.HealthPercent <= 90 && this.Target.DistanceToPlayer <= 8)
             {           
                this.Player.Cast("Psychic Scream"); 
                }
            if  (this.Player.GetSpellRank("Devouring Plague") != 0 && this.Attackers.Count >= 2 && this.Player.CanUse("Devouring Plague") && this.Player.ManaPercent >= 50 && this.Target.HealthPercent >= 50)
            {
                this.Player.Cast("Devouring Plague"); 
            }
           if (this.Player.GetSpellRank("Renew") != 0 && this.Attackers.Count >= 2 && this.Player.ManaPercent >= 50 && this.Player.HealthPercent <= 80 && !this.Player.GotBuff("Renew") && !useShadowForm)
            {
                this.Player.Cast("Shadowform");
                this.Player.CastWait("Renew", 1000);
            } 
        }

        public void OffensiveSpells() 
        {   //Make sure to have PW:S up before trying any funky attack stuff after pulling
            if (!this.Player.GotBuff("Power Word Shield") && !this.Player.GotDebuff("Weakened Soul") && this.Player.CanUse("Power Word:Shield"))
            {
                this.Player.Cast("Power Word:Shield");
            }
            

        	/*if (!this.Target.GotDebuff("Shadow Word: Pain") ||
        		(this.Player.CanUse("Mind Flay") && this.Target.HealthPercent >= healthP) ||
        		(this.Player.CanUse("Vampiric Embrace") && !this.Target.GotDebuff("Vampiric Embrace")) ||
        		(this.Target.HealthPercent > 95 && this.Player.CanUse("Mind Blast")))
        	{*/
        		if(debug==true)
	            	{
	            		this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage('trying to reapply dots')");
	            	}
	        	if (!this.Player.GotBuff("Shadowform") && this.Player.GetSpellRank("Shadowform") != 0)
	            {
	                this.Player.Cast("Shadowform");
	            }

	            if (this.Player.GetSpellRank("Shadow Word: Pain") != 0 && this.Target.HealthPercent >= 5 && this.Player.ManaPercent >= 10 && this.Player.IsChanneling == "" && this.Player.IsCasting == "")
	            {
	                if (!this.Target.GotDebuff("Shadow Word: Pain"))
	                {
	                    
	                    this.Player.Cast("Shadow Word: Pain");
	                }
	            }

	            if (this.Player.CanUse("Berserking"))
	            {
	            	this.Player.TryCast("Berserking");
	            }

                

	            if (this.Target.HealthPercent > 95 && this.Player.GetSpellRank("Mind Blast") != 0 && this.Player.CanUse("Mind Blast"))
	            {	//In case we bodypull / get a second enemy while fighting, the precast mind blast wont happen.
	            	this.Player.Cast("Mind Blast");
	            }

	            if(useVEmb == true)
	            {
					if (this.Player.GetSpellRank("Vampiric Embrace") != 0)
		            {
		                if (!this.Target.GotDebuff("Vampiric Embrace"))
		                {
		                    
		                    this.Player.Cast("Vampiric Embrace");
		                }
		            }
	        	}

	            if (this.Player.GetSpellRank("Mind Flay") != 0)
	            {
	                if (this.Player.CanUse("Mind Flay") && this.Target.HealthPercent >= healthP && this.Player.IsChanneling != "Mind Flay" && this.Player.IsCasting != "Mind Flay" && this.Player.ManaPercent >= 10)
	                {
	                    this.Player.StopWand();
	                    this.Player.CastWait("Mind Flay", 2500);
	                }
	            }
	        //}
        }

        public void DefensiveSpells()
        {

        	if((!this.Player.GotBuff("Power Word: Shield") && !this.Player.GotDebuff("Weakened Soul")) ||
        		this.Player.HealthPercent <= 40 ||
        		!this.Player.GotBuff("Inner Fire"))
        	{
        		
        		if(debug==true)
	            	{
	            		this.Player.DoString("DEFAULT_CHAT_FRAME:AddMessage('trying to reapply shield or heal')");
	            	}
	        	if (this.Player.GetSpellRank("Power Word: Shield") != 0)
	            {
	                if (!this.Player.GotBuff("Power Word: Shield") && !this.Player.GotDebuff("Weakened Soul") && this.Player.IsChanneling != "Mind Flay" && this.Player.IsCasting != "Mind Flay" && this.Player.ManaPercent >= 10)
	                {
	                    
	                    this.Player.Cast("Power Word: Shield");
	                }
	            }

				if (this.Player.HealthPercent <= 40)
	            {
	            	if (this.Player.GetSpellRank("Flash Heal") != 0 && this.Player.ManaPercent >= 60)
	                {
	                	if (this.Player.GotBuff("Shadowform"))
	                    {
	 						this.Player.Cast("Shadowform");
	                    }
	                   
	                    this.Player.CastWait("Flash Heal", 1000);
	                }
	                else
	                {
	                	if (this.Player.GetSpellRank("Heal") != 0)
	                    {
	                    	this.Player.CastWait("Heal", 1000);
	                    }
	                    this.Player.CastWait("Lesser Heal", 1000);
	                }
	            }

	            if (this.Player.GetSpellRank("Inner Fire") != 0)
	            {
	                if (!Player.GotBuff("Inner Fire"))
	                {
	                    this.Player.Cast("Inner Fire");
	                }
	            }
        	}
        }


        

        public override void PreFight()
        {
            //You can change this value if you have specced into Shadow Reach: 1/3 = 30*1.06 = 32 , 2/3 = 30*1.13 = 34 , 3/3 = 30*1.2 = 36
            this.SetCombatDistance(30);
            pullPriority();
           
        }

        public override void Fight()
        {
            
            bool canWand = this.Player.IsWandEquipped();

            MultipleEnemies();
	        DefensiveSpells();
            OffensiveSpells();
            MultiDotting();
            SelectMPotion();
            SelectHPotion();

           if (useSilence == true)
           {
           	SilenceEnemy();
           }

           
           if (canWand == true && useWand == true && this.Player.CanUse("Shadow Word: Pain") && this.Player.IsCasting == "" && this.Player.IsChanneling == "")
           {
           		gotWand();
           }

           if (canWand == false || useWand == false)
           {
           		noWand();
           }
           
        }



            
		public override void Rest()
        {

            if (this.Player.ManaPercent < 20 & !this.Player.GotBuff("Drink"))
            {
                SelectDrink();
                return;
            }
            else
            {
              /*   Taken out right now, did not seem too useful.
                if (this.Player.HealthPercent <= 80 && this.Player.GetSpellRank("Flash of Light") != 0 && !this.Player.GotBuff("Drink") && this.Player.ManaPercent >= 70)
                {
                    this.Player.CastWait("Lesser Heal", 1000);
                }
                else if (this.Player.HealthPercent <= 75 && !this.Player.GotBuff("Drink") && this.Player.ManaPercent >= 50)
                {
                    if (this.Player.GetSpellRank("Heal") != 0)
                    {
                        this.Player.CastWait("LesserHeal", 1000);
                    }
                    else
                    {
                        this.Player.CastWait("Lesser Heal", 1000);
                    }
                } */
            }
            
        }


        public override bool Buff()
        {
            try {

            if (this.Player.GetSpellRank("Touch of Weakness") != 0 && useTouchOfWeakness == true)
            {
                if (!this.Player.GotBuff("Touch of Weakness"))
                {
                    this.Player.Cast("Touch of Weakness");
                    return false;
                }
            }
            if (this.Player.GetSpellRank("Inner Fire") != 0)
            {
                if (!this.Player.GotBuff("Inner Fire"))
                {
                    this.Player.Cast("Inner Fire");
                    return false;
                }
            }
            if (this.Player.GetSpellRank("Power Word: Fortitude") != 0)
            {
                if (!this.Player.GotBuff("Power Word: Fortitude"))
                {
                    this.Player.Cast("Power Word: Fortitude");
                    return false;
                }
            }
            if (this.Player.GetSpellRank("Divine Spirit") != 0)
            {
                if (!this.Player.GotBuff("Divine Spirit"))
                {
                    this.Player.Cast("Divine Spirit");
                    return false;
                }
            }

            if (useShadowForm == true && !this.Player.GotBuff("Shadowform") && this.Player.CanUse("Shadowform"))
            {
                this.Player.Cast("Shadowform");
                return false;
            }
                
            this.talentManager.DoWork();
        }catch (Exception){}
            //True means we are done buffing, or cannot buff
            return true;
        }
    

public class Talent {
   public string Name {
    get;
    private set;
   }
   public int CurrentRank {
    get;
    private set;
   }
   public int MaxRank {
    get;
    private set;
   }
   public int Tab {
    get;
    private set;
   }
   public int Index {
    get;
    private set;
   }
   public Talent(string name, int currentRank, int maxRank, int tab, int index) {
    this.Name = name;
    this.CurrentRank = currentRank;
    this.MaxRank = maxRank;
    this.Tab = tab;
    this.Index = index;
   }
  }
  public class IndexModel {
   public int Index {
    get;
    set;
   }
  }
  public class TalentManager {
   private
   const string KeyUnspentTalents = "unspentTalents";
   private
   const string KeyTalentDictionary = "talentDictionary";
   private readonly _Player me;
   public TalentManager(_Player me) {
    this.me = me;
   }
   public void DoWork() {
    var unspentTalents = this.GetUnspentTalents();
    if (unspentTalents == 0) {
     return;
    }
    var talents = this.GetTalents();
    for (int i = 0; i < Constants.TalentStrings.Length; i++) {
     var talentString = Constants.TalentStrings[i];
     for (int j = 0; j < talentString.Length; j++) {
      var c = talentString.Substring(j, 1);
      var number = Convert.ToInt32(c);
      if (number > talents[j].CurrentRank && number <= talents[j].MaxRank) {
       this.me.DoString(string.Format("LearnTalent({0}, {1});", talents[j].Tab, talents[j].Index));
       ClearCache();
       return;
      }
     }
    }
   }
   private int GetUnspentTalents() {
    return Cache.Instance.GetOrStore(KeyUnspentTalents, () => {
     this.me.DoString("TM_unspentTalentPoints, TM_learnedProfessions = UnitCharacterPoints(\"player\");");
     return new IndexModel {
      Index = Convert.ToInt32(this.me.GetText("TM_unspentTalentPoints"))
     };
    }, 120).Index;
   }
   private IList < Talent > GetTalents() {
    return Cache.Instance.GetOrStore(KeyTalentDictionary, () => {
     var talents = new List < Talent > ();this.me.DoString("TM_numberOfTabs = GetNumTalentTabs()");int tabCount = Convert.ToInt32(this.me.GetText("TM_numberOfTabs"));
     for (int i = 1; i <= tabCount; i++) {
      this.me.DoString(string.Format("TM_numberOfTalents = GetNumTalents({0})", i));
      int talentCount = Convert.ToInt32(this.me.GetText("TM_numberOfTalents"));
      for (int j = 1; j <= talentCount; j++) {
       this.me.DoString(string.Format("TM_nameTalent, TM_icon, TM_tier, TM_column, TM_currRank, TM_maxRank = GetTalentInfo({0},{1});", i, j));
       var talent = new Talent(this.me.GetText("TM_nameTalent"), Convert.ToInt32(this.me.GetText("TM_currRank")), Convert.ToInt32(this.me.GetText("TM_maxRank")), i, j);
       talents.Add(talent);
      }
     }
     return talents;
    }, maxDuration: 120);
   }
   private void ClearCache() {
    Cache.Instance.RemoveFromCache(KeyTalentDictionary);
    Cache.Instance.RemoveFromCache(KeyUnspentTalents);
   }
  }
  internal class CacheItem {
   public object StoredObject {
    get;
    private set;
   }
   public DateTime Time {
    get;
    private set;
   }
   public CacheItem(object obj) {
    this.StoredObject = obj;
    this.Time = DateTime.UtcNow;
   }
  }
  internal class Cache {
   private static readonly Lazy < Cache > instance = new Lazy < Cache > (() => new Cache());
   private static object lockert = new object();
   private readonly Hashtable cache;
   private Cache() {
    cache = new Hashtable();
   }
   public static Cache Instance {
    get {
     return instance.Value;
    }
   }
   public T GetOrStore < T > (string key, Func < T > action, int maxDuration = -1) where T: class {
    var result = this.cache[key];
    if (result == null || (maxDuration > 0 && DateTime.UtcNow > ((CacheItem) result).Time.AddSeconds(maxDuration))) {
     lock(lockert) {
      if (result == null || (maxDuration > 0 && DateTime.UtcNow > ((CacheItem) result).Time.AddSeconds(maxDuration))) {
       var obj = action();
       result = obj != null ? new CacheItem(obj) : new CacheItem(
        default (T));
       this.cache[key] = result;
      }
     }
    }
    if (result == null) {
     return default (T);
    }
    return (T)((CacheItem) result).StoredObject;
   }
   public void RemoveFromCache(string key) {
    if (this.cache.ContainsKey(key)) {
     this.cache.Remove(key);
    }
   }
  }
  }
}
