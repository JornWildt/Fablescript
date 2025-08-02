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
  print("DIR: " .. direction)
  return find_object_by_name(exits, direction)
end


function find_object_by_name(list, name)
  if not list then return nil end

  local lname = string.lower(name)
  for _, item in ipairs(list) do
    if type(item.name) == "string" and string.lower(item.name) == lname then
      return item
    end
  end
  return nil
end


function describe_scene()
  local location = Player.location
  if location then
    local facts = Fun.map(
      location.facts or {}, 
      function(f) return { f.text } end)

    local exits = Fun.map(
      location.exits or {}, 
      function(x) return { Title = x.title, Description = x.description } end)

    local objects_here = Fun.filter(
      location.objects_here or {},
      function(o) return o ~= Player end)

    objects_here = Fun.map(
      objects_here or {},
      function(o) return { Name = o.name, Title = o.title, Description = o.description } end)

    local args = {
      Title = location.title,
      Introduction = location.introduction,
      Facts = facts,
      HasFacts = next(facts) ~= nil,
      Exits = exits,
      HasExits = next(exits) ~= nil,
      Objects = objects_here,
      HasObjects = next(objects_here) ~= nil
    }
    print("Describe: " .. args.Title)
    local output = Core.run_prompt("DescribeScene", args)
    return output
  else
    return "You are in the void"
  end
end