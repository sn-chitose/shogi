package sample.tiles;

/**
 * Created by Alex on 23.09.2015.
 */
public class Hisha extends Tile {

    public Hisha(int x, int y, boolean player2) {
        super(x, y, player2);
    }

    @Override
    protected void initMoves() {
        moves = new int[]{22,44,66,88};
        promoted = new int[]{1,3,7,9,22,44,66,88};
    }

    @Override
    public String getTile() {
        return "hi";
    }
}
