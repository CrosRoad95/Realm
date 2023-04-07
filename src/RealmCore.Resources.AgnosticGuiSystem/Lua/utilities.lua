local function createGrid(sx, sy, columns, rows, cellsPadding)
  local grid = {}
  local cellx = (sx - (columns - 1) * cellsPadding) / columns
  local celly = (sy - (rows - 1) * cellsPadding) / rows
  local px,py = 0,0
  for x=0, columns - 1 do
    for y=0, rows - 1 do
      py = py + celly + cellsPadding
      grid[#grid + 1] ={x + 1,y + 1,px,py}
    end
    px = px + cellx + cellsPadding
    py = 0
  end
  return grid, cellx, celly;
end

utilities = {
    createGrid = createGrid
}