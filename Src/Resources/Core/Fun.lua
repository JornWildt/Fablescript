Fun = {}

function Fun.map(tbl, func)
  local result = {}
  for i, v in ipairs(tbl) do
    result[i] = func(v, i)
  end
  return result
end
