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


function find_exit_by_direction(exits, direction)
  print(exits)
  print("DIR: " .. direction)
  for _, exit in ipairs(exits) do
    if exit.name == direction then
      return exit
    end
  end
  return nil
end