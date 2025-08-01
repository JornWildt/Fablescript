dofile("Utilities.lua")
dofile("Object.lua")
dofile("Core.lua")

x = { { text = "A" }, { text = "B" } }
facts = Fun.map(
  x or {}, 
  function(x) return { x.text } end)


--Player = BaseObject:new({ name = "McWurzt" })
--Beach = BaseObject:new({ name = "The beach" })
--Town = BaseObject:new({ name = "Wild City" })
--Knife = BaseObject:new({ name = "Knife" })

--Player:inspect()
--Knife:inspect()
--Beach:inspect()
--Town:inspect()

--Player:move_to(Beach)
--Player:inspect()
--Knife:inspect()
--Beach:inspect()
--Town:inspect()

--Player:move_to(Town)
--Knife:move_to(Player)
--Player:inspect()
--Knife:inspect()
--Beach:inspect()
--Town:inspect()
