script_name = "Replace markers by styles"
script_description = "Assigns styles based on prefix markers"
script_author = "arc_"

re = require("aegisub.re")

function run(subtitles, markerStyles)
    for i = 1, #subtitles do
        local line = subtitles[i]
        if line.class == "dialogue" and not line.comment then
            subtitles[i] = handleLine(line, markerStyles)
        end
    end
end

function handleLine(line, markerStyles)
    local marker
    local style
    
    for m, s in pairs(markerStyles) do
        if #line.text >= #m and line.text:sub(1, #m) == m then
            marker = m
            style = s
            break
        end
    end
    
    if style then
        line.text = line.text:sub(#marker + 1)
        line.style = style
    end
    
    return line
end

aegisub.register_macro(
    "Replace markers/HimeHina",
    script_description,
    function(subtitles)
        run(
            subtitles,
            {
                ["<"] = "Hime",
                [">"] = "Hina"
            }
        )
    end
)

aegisub.register_macro(
    "Replace markers/Eilene",
    script_description,
    function(subtitles)
        run(
            subtitles,
            {
                ["*"] = "YomemiMoemi",
                ["+"] = "Eilene",
                ["/"] = "Beilene",
                ["^"] = "Beno",
                ["&"] = "Étra"
            }
        )
    end
)

aegisub.register_macro(
    "Replace markers/Moe",
    script_description,
    function(subtitles)
        run(
            subtitles,
            {
                ["*"] = "Moe",
                ["+"] = "Killjoy",
                ["/"] = "Pending"
            }
        )
    end
)


aegisub.register_macro(
    "Replace markers/Sifir",
    script_description,
    function(subtitles)
        run(
            subtitles,
            {
                ["*"] = "Renana",
                ["+"] = "Sifir"
            }
        )
    end
)

aegisub.register_macro(
    "Replace markers/Idolbu",
    script_description,
    function(subtitles)
        run(
            subtitles,
            {
                ["az-"] = "Azuki",
                ["ba-"] = "Baacharu",
                ["ch-"] = "Chieri",
                ["fu-"] = "Futaba",
                ["io-"] = "Iori",
                ["ir-"] = "Iroha",
                ["me-"] = "Mememe",
                ["mm-"] = "MerryMilk",
                ["mo-"] = "Mochi",
                ["na-"] = "Natori",
                ["pi-"] = "Pino",
                ["ri-"] = "Riko",
                ["si-"] = "Siro",
                ["su-"] = "Suzu",
                ["ta-"] = "Tama"
            }
        )
    end
)

