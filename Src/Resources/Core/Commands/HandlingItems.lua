
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



function Commands.look_at(objName)
  print("You see: " .. objName)
end

