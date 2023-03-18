namespace DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking.Data;

internal static class RelativeTimeSpanNer
{
    public static string Declaration = 
        "using System;\r\n" +
        "\r\n" +
        "// root expression\r\n" +
        "TimeSpan Root = peg#($BeforeOrAfter:multiplier $TimeSpan:timeSpan)# {\r\n" +
        "    return timeSpan.Multiply(multiplier);\r\n" +
        "}\r\n" +
        "\r\n" +
        "// direction modifier\r\n" +
        "int BeforeOrAfter = peg#($Before:before|$After:after)# { return (before ?? 0) + (after ?? 0); }\r\n" +
        "int Before = peg#(за)# { return -1; }\r\n" +
        "int After = peg#(через)# { return 1; }\r\n" +
        "\r\n" +
        "// alternatives of possible relative time phrase\r\n" +
        "TimeSpan TimeSpan = peg#($HoursExact:hoursExact|$HoursAndMinutes_WithWords:hmWithWords|$HoursAndMinutes_WithoutWords:hmNoWords|$HoursOnly_WithWord:h|$MinutesOnly_WithWord:m)# { return (hoursExact ?? hmWithWords ?? hmNoWords ?? h ?? m).Value; }\r\n" +
        "\r\n" +
        "// different patterns for relative time phrase\r\n" +
        "TimeSpan HoursExact = peg#($HoursNumber_HalfAnHour:special|$HoursExact_WithHalf:common)# { return (special ?? common).Value; }\r\n" +
        "TimeSpan HoursExact_WithHalf = peg#($Doubles_Halves_After_0_12:hours $Word_Hour__S_Nom_S_Gen_P_Gen)# { return TimeSpan.FromHours(hours); }\r\n" +
        "TimeSpan HoursAndMinutes_WithWords = peg#($HoursNumber_WithWord:hours $MinutesNumber_WithWord:minutes)# { return hours.Add(minutes); }\r\n" +
        "TimeSpan HoursAndMinutes_WithoutWords = peg#($HoursNumber_WithoutExtraWord:hours $MinutesNumber_WithoutWord:minutes)# { return hours.Add(minutes); }\r\n" +
        "TimeSpan HoursOnly_WithWord = peg#($HoursNumber_WithWord:hours)# { return hours; }\r\n" +
        "TimeSpan MinutesOnly_WithWord = peg#($MinutesNumber_WithWord:minutes)# { return minutes; }\r\n" +
        "\r\n" +
        "// minutes helpers\r\n" +
        "TimeSpan MinutesNumber_WithWord = peg#($MinutesNumber_WithoutWord:number $Word_Minute__S_Nom_S_Gen_P_Gen)# { return number; }\r\n" +
        "TimeSpan MinutesNumber_WithoutWord = peg#($Integer_0? $Integers_0_9__F_Acc:n_0_9|$Integers_10_19:n_10_19|$Integers_20_59__F_Acc:n_20_59)# { return TimeSpan.FromMinutes((n_0_9 ?? n_10_19 ?? n_20_59).Value); }\r\n" +
        "\r\n" +
        "// hours helpers\r\n" +
        "TimeSpan HoursNumber_WithWord = peg#($HoursNumber_WithoutWord:common $Word_Hour__S_Nom_S_Gen_P_Gen|$HoursNumber_SingleHour:special)# { return (common ?? special).Value; }\r\n" +
        "TimeSpan HoursNumber_WithoutWord = peg#($Integers_0_12__M_Acc:hours)# { return TimeSpan.FromHours(hours); }\r\n" +
        "TimeSpan HoursNumber_WithoutExtraWord = peg#($HoursNumber_WithoutWord:hours|$HoursNumber_SingleHour:hour)# { return (hours ?? hour).Value; }\r\n" +
        "TimeSpan HoursNumber_SingleHour = peg#(час)# { return TimeSpan.FromHours(1); }\r\n" +
        "TimeSpan HoursNumber_HalfAnHour = peg#(полчаса)# { return TimeSpan.FromMinutes(30); }\r\n" +
        "\r\n" +
        "// numeric helpers\r\n" +
        "// legend:\r\n" +
        "// if the word doesn't distinguish significant (in the context of current PEG-grammar) grammatical categories\r\n" +
        "// (such as declension, grammatical gender or grammatical number),\r\n" +
        "// the respective rule is not being marked with additional suffix, otherwise it is:\r\n" +
        "// \"F\" and \"M\" stand for feminine and masculine grammatical genders respectively\r\n" +
        "// \"S\" and \"P\" stand for singular and plural grammatical numbers respectively\r\n" +
        "// \"Nom\" and \"Acc\" stand for nominative and accusative cases respectively\r\n" +
        "int Integers_0_9__F_Acc = peg#($Integer_0:n_0|$Integers_1_9__F_Acc:n_1_9)# { return (n_0 ?? n_1_9).Value; }\r\n" +
        "int Integers_1_9__F_Acc = peg#($Integer_1__F_Acc:n_1|$Integer_2__F_Acc:n_2|$Integers_3_9:n_3_9)# { return (n_1 ?? n_2 ?? n_3_9).Value; }\r\n" +
        "int Integers_20_59__F_Acc = peg#($Integers_Tens_20_50:tens $Integers_1_9__F_Acc?:ones)# { return tens + (ones ?? 0); }\r\n" +
        "int Integers_0_9__M_Acc = peg#($Integer_0:n_0|$Integer_1__M_Acc:n_1|$Integer_2__M_Acc:n_2|$Integers_3_9:n_3_9)# { return (n_0 ?? n_1 ?? n_2 ?? n_3_9).Value; }\r\n" +
        "int Integers_0_12__M_Acc = peg#($Integers_0_9__M_Acc:n_0_9|$Integer_10:n_10|$Integer_11:n_11|$Integer_12:n_12)# { return (n_0_9 ?? n_10 ?? n_11 ?? n_12).Value; }\r\n" +
        "int Integers_3_9 = peg#($Integer_3:n_3|$Integer_4:n_4|$Integer_5:n_5|$Integer_6:n_6|$Integer_7:n_7|$Integer_8:n_8|$Integer_9:n_9)# { return (n_3 ?? n_4 ?? n_5 ?? n_6 ?? n_7 ?? n_8 ?? n_9).Value; }\r\n" +
        "int Integers_10_19 = peg#($Integer_10:n_10|$Integer_11:n_11|$Integer_12:n_12|$Integer_13:n_13|$Integer_14:n_14|$Integer_15:n_15|$Integer_16:n_16|$Integer_17:n_17|$Integer_18:n_18|$Integer_19:n_19)# { return (n_10 ?? n_11 ?? n_12 ?? n_13 ?? n_14 ?? n_15 ?? n_16 ?? n_17 ?? n_18 ?? n_19).Value; }\r\n" +
        "\r\n" +
        "int Integer_0 = peg#(ноль)# => 0\r\n" +
        "int Integer_1__M_Acc = peg#(один)# => 1\r\n" +
        "int Integer_1__F_Acc = peg#(одну)# => 1\r\n" +
        "int Integer_2__M_Acc = peg#(два)# => 2\r\n" +
        "int Integer_2__F_Acc = peg#(две)# => 2\r\n" +
        "int Integer_3 = peg#(три)# => 3\r\n" +
        "int Integer_4 = peg#(четыре)# => 4\r\n" +
        "int Integer_5 = peg#(пять)# => 5\r\n" +
        "int Integer_6 = peg#(шесть)# => 6\r\n" +
        "int Integer_7 = peg#(семь)# => 7\r\n" +
        "int Integer_8 = peg#(восемь)# => 8\r\n" +
        "int Integer_9 = peg#(девять)# => 9\r\n" +
        "int Integer_10 = peg#(десять)# => 10\r\n" +
        "int Integer_11 = peg#(одиннадцать)# => 11\r\n" +
        "int Integer_12 = peg#(двенадцать)# => 12\r\n" +
        "int Integer_13 = peg#(тринадцать)# => 13\r\n" +
        "int Integer_14 = peg#(четырнадцать)# => 14\r\n" +
        "int Integer_15 = peg#(пятнадцать)# => 15\r\n" +
        "int Integer_16 = peg#(шестнадцать)# => 16\r\n" +
        "int Integer_17 = peg#(семьнадцать)# => 17\r\n" +
        "int Integer_18 = peg#(восемьнадцать)# => 18\r\n" +
        "int Integer_19 = peg#(девятнадцать)# => 19\r\n" +
        "\r\n" +
        "int Integers_Tens_20_50 = peg#($Integers_Tens_20:n_20|$Integers_Tens_30:n_30|$Integers_Tens_40:n_40|$Integers_Tens_50:n_50)# { return (n_20 ?? n_30 ?? n_40 ?? n_50).Value; }\r\n" +
        "int Integers_Tens_20 = peg#(двадцать)# => 20\r\n" +
        "int Integers_Tens_30 = peg#(тридцать)# => 30\r\n" +
        "int Integers_Tens_40 = peg#(сорок)# => 40\r\n" +
        "int Integers_Tens_50 = peg#(пятьдесят)# => 50\r\n" +
        "\r\n" +
        "double Doubles_Halves_After_0_12 = peg#($Doubles_Halves_After_0_Special:n_0|$Doubles_Halves_After_1_Special:n_1|$Doubles_Halves_After_0_12_Common:n_0_12)# { return (n_0 ?? n_1 ?? n_0_12).Value; }\r\n" +
        "double Doubles_Halves_After_0_Special = peg#(пол)# { return 0.5; }\r\n" +
        "double Doubles_Halves_After_1_Special = peg#(полтора)# { return 1.5; }\r\n" +
        "double Doubles_Halves_After_0_12_Common = peg#($Integers_0_12__M_Acc:n с половиной)# { return n + 0.5; }\r\n" +
        "\r\n" +
        "// lexical helpers\r\n" +
        "void Word_Minute__S_Nom_S_Gen_P_Gen = peg#([минута минуты минут])# => void\r\n" +
        "void Word_Hour__S_Nom_S_Gen_P_Gen = peg#([час часа часов])# => void";
}