public static class TraitHelper
{
    public static string GetTraitName(TraitType trait) => trait switch
    {
        TraitType.Ace => "에이스",
        TraitType.Fragile => "허약 체질",
        TraitType.BurnoutProne => "번아웃 체질",
        TraitType.Overenthusiast => "의욕 과다",
        TraitType.Ideaman => "아이디어맨",
        TraitType.Troublemaker => "갈등 유발",
        TraitType.CoffeeAddict => "커피중독자",
        TraitType.MoodMaker => "분위기 메이커",
        TraitType.Loner => "혼자가 좋아",
        TraitType.GlassMental => "유리멘탈",
        TraitType.Optimist => "긍정이",
        TraitType.MorningPerson => "아침형 인간",
        TraitType.NightOwl => "야행성",
        TraitType.Workaholic => "일중독",
        TraitType.Pessimist => "부정형 인간",
        _ => ""
    };

    // 특성 배열 → 구분자로 이어붙인 문자열
    public static string GetTraitsString(TraitType[] traits, string separator = ", ")
    {
        var result = "";
        foreach (var t in traits)
        {
            var name = GetTraitName(t);
            if (!string.IsNullOrEmpty(name))
                result += name + separator;
        }
        return result.TrimEnd(separator.ToCharArray());
    }
}
