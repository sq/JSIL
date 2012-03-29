using System;
using JSIL.Meta;

public static class Program {
    public enum Stat {
        HP, MP, STR, END, MAG, MGR, HIT, DOD, STK, FER, REA, CTR, ATK, DEF
    };

    public static Stat getStat (string s) {
        switch (s) {
            case "HP": return Stat.HP;
            case "MP": return Stat.MP;
            case "STR": return Stat.STR;
            case "END": return Stat.END;
            case "MAG": return Stat.MAG;
            case "MGR": return Stat.MGR;
            case "HIT": return Stat.HIT;
            case "DOD": return Stat.DOD;
            case "STK": return Stat.STK;
            case "FER": return Stat.FER;
            case "REA": return Stat.REA;
            case "CTR": return Stat.CTR;
            case "ATK": return Stat.ATK;
            case "DEF": return Stat.DEF;
            default:
                throw new Exception("Unknown stat '" + s + "'");
        }
    }

    public static void Main (string[] args) {
        foreach (var arg in args) {
            Console.WriteLine(getStat(arg));
        }
    }
}