import React, { useMemo } from 'react';
import CarouselSection from '../components/CarouselSection';
import '../css/Home2.css';

const Home = ({ products, onViewProduct, onOrderNow }) => {
    const data = products || {};
    const freshBestsellers = useMemo(() => {
        const currentBestsellers = data.bestsellers || [];
        const allInventory = [];
        Object.keys(data).forEach(key => {
            if (key !== 'bestsellers' && Array.isArray(data[key])) {
                allInventory.push(...data[key]);
            }
        });

        return currentBestsellers.map(bestsellerItem => {
            const updatedItem = allInventory.find(item => item.id === bestsellerItem.id);
            return updatedItem || bestsellerItem;
        });
    }, [data]);

    return (
        <>
            <header className="hero">
                <div className="hero-content">
                    <h1>Where great coffee<br />meets good vibes.</h1>
                    <button className="order-btn" onClick={onOrderNow}>Order Now</button>
                </div>
            </header>

            <main>
                <CarouselSection
                    title="Bestsellers You Can't Go Wrong With"
                    subtitle="Your soon-to-be favorites, too."
                    items={freshBestsellers}
                    onViewProduct={onViewProduct}
                />

                <CarouselSection
                    title="Beverages"
                    subtitle="From mellow mornings to fizzy afternoons."
                    items={data.frappe || []}
                    onViewProduct={onViewProduct}
                />

                <CarouselSection
                    title="Snacks & Desserts"
                    subtitle="Sweet or savory, every bite is a happiness."
                    items={data.baked || []}
                    onViewProduct={onViewProduct}
                />
            </main>
        </>
    );
};
export default Home;

