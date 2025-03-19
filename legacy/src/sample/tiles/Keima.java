package sample.tiles;

/**
 * Created by Alex on 23.09.2015.
 */
public class Keima extends Tile {


    public Keima(int x, int y, boolean player2) {
        super(x, y, player2);
    }

    @Override
    protected void initMoves() {
        moves = new int[]{27,29};
        promoted = new int[]{2,4,6,7,8,9};
    }

    @Override
    public String getTile() {
        return "kei";
    }
}
