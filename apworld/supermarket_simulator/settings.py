import settings

class SupermarketSettings(settings.Group):
    class AllowBelowLocalCheckoutFillMinimums(settings.Bool):
        """Allows player YAMLs to bypass local checkout fill safety minimums."""

    allow_below_local_checkout_fill_minimums: (
        AllowBelowLocalCheckoutFillMinimums | bool
    ) = False
