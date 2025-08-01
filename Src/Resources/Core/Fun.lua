Fun = {}

function Fun.filter(tbl, pred)
  local result = {}
  for i, v in ipairs(tbl) do
    if pred(v, i) then
      table.insert(result, v)
    end
  end
  return result
end

function Fun.map(tbl, func)
  local result = {}
  for i, v in ipairs(tbl) do
    result[i] = func(v, i)
  end
  return result
end
