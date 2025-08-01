Commands = {}


function Commands.inspect(objName)
  print('INSPECT: ' .. objName)
  local obj = Objects[objName];
  if obj then
    Core.say("'" .. obj.name .. "' is at '" .. ((obj.location and obj.location.name) or "<nowhere>") .. "'")
    for i, v in ipairs(obj.objects_here) do
      Core.say("  '" .. v.name .. "' is here")
    end
  else
    Core.say("Unknown object.");
  end
end


function Commands.take(objName)
  print('TAKE: ' .. objName)
  local obj = Objects[objName];
  obj:inspect()

  Player:inspect();
  obj:move_to(player)
  Core.say("You took the " .. obj.title)
end


function Commands.drop(objName)
  print('DROP: ' .. objName)
  Core.say("You tried to drop it, but inventory is not implemented")
end


function Commands.look()
  Core.describe_scene()
end


function Commands.go(direction)
  print('GO: ' .. direction)
  Core.say("Try go to: " .. direction)
  local exit = find_exit_by_direction(Player.location.exits, direction)
  local destination = exit.targetLocation
  Player:move_to(destination);
  Player:inspect()
  destination:inspect()
  Core.describe_scene()
end


function Commands.look_at(objName)
  print("You see: " .. objName)
end

