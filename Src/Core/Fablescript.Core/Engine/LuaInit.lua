-- Core base class system
function createPrototype(base)
    local prototype = base or {}
    prototype.__index = prototype
    function prototype:new(o)
        o = o or {}
        setmetatable(o, self)
        print('HELLO: ' .. self.Name)
        return o
    end
    return prototype
end

-- Game object prototype
GameObject = createPrototype()

GameObject.Name = "Unnamed"
GameObject.Description = "No description."
GameObject.Hugo = "H.u.g.o"

function GameObject:Inspect()
    print("You see a " .. self.Name .. ": " .. self.Description .. "(" .. self.Hugo .. ")")
end

tmp = GameObject:new()
tmp.Name = 'AAA'
print('tmp: ' .. tmp.Name)
tmp:Inspect()