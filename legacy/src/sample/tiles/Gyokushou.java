package sample.tiles;

/**
 * Created by Alex on 23.09.2015.
 */
public class Gyokushou extends Tile {

    public Gyokushou(int x, int y, boolean player2) {
        super(x, y, player2);
    }

    @Override
    protected void initMoves() {
        moves = new int[]{1,2,3,4,6,7,8,9};
    }

    @Override
    public String getTile() {
        return "gyoku";
    }
}
