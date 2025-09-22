get dwg/_search?filter_path=*.*.*.*.style,*.*.*.file,*.*.*.*.value*,*.*.*.content
{
  "query": {"bool": {
    "must": [
    {"terms": {"contentaslist.style": 
    ["dcadtxtstyle1","dcadtxtstyle2","dcadtxtstyle3","dcadtxtstyle4",
    "dcadtxtstyle5","dcadtxtstyle6","dcadtxtstyle7","dcadtxtstyle8",
    "dcadtxtstyle9","dcadtxtstyle10","dcadtxtstyle11","dcadtxtstyle12",
    "dcadtxtstyle13","dcadtxtstyle14","dcadtxtstyle15","dcadtxtstyle16",
    "dcadtxtstyle17","dcadtxtstyle18","dcadtxtstyle19","dcadtxtstyle20",
    "dcadtxtstyle21","dcadtxtstyle22","dcadtxtstyle23","dcadtxtstyle24"]}}
      ]
    }
  },
  "size": 1000
}