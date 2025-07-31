function create_prototype(base)
  local prototype = base or {}
  prototype.__index = prototype

  function prototype:new(o)
    o = o or {}
    setmetatable(o, self)
    if self.init then self.init(o) end
    return o
  end

  return prototype
end


function remove_reference_from_list(list, target)
  for i, v in ipairs(list) do
    if v == target then
      table.remove(list, i)
      return true
    end
  end
end

