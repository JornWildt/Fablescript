function Commands.look()
  Core.say(describe_scene());
end


function Commands.go(direction)
  print('GO: ' .. direction)
  local exit = find_exit_by_direction(Player.location.exits, direction)
  if exit then
    local destination = exit.targetLocation
    Player:move_to(destination)
    Player:inspect()
    destination:inspect()
    Core.say(describe_scene())
  else
    Core.say('No such direction')
  end
end
