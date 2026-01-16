import React, { useState } from 'react';
import ItemList from '../components/ItemList';
import MenuHeader from '../components/MenuHeader';
import EditItemAdmin from '../components/EditItemAdmin';
import AddItemAdmin from '../components/AddItemAdmin'; // 

const AdminMenu = ({ products, onSave, onViewProduct }) => {
    const [activeCategory, setActiveCategory] = useState('classic');
    const [editingItem, setEditingItem] = useState(null);
    const [showAddModal, setShowAddModal] = useState(false);
    const handleAddItem = () => {
        setShowAddModal(true);
    };

    const handleEditProduct = (item) => {
        setEditingItem(item);
    };

    const handleCloseEdit = () => {
        setEditingItem(null);
    };

    const handleSaveAndClose = (updatedItem) => {
        if (onSave) {
            onSave(updatedItem);
        }
        setEditingItem(null);
    };

    const handleCloseAdd = () => {
        setShowAddModal(false);
    };

    const handleAddSave = () => {
        if (onSave) onSave(); // Refresh data
        setShowAddModal(false);
    };

    const categories = [
        { id: 'classic', label: 'Classic Coffee Series' },
        { id: 'frappe', label: 'Frappe' },
        { id: 'latte', label: 'Latte' },
        { id: 'specialty', label: 'Specialty Drinks' },
        { id: 'baked', label: 'Cupcakes & Baked Goods' },
    ];

    const scrollToSection = (id) => {
        const element = document.getElementById(id);
        if (element) {
            const headerOffset = 180;
            const elementPosition = element.getBoundingClientRect().top;
            const offsetPosition = elementPosition + window.pageYOffset - headerOffset;
            window.scrollTo({ top: offsetPosition, behavior: "smooth" });
            setActiveCategory(id);
        }
    };

    return (
        <div className="menu-page">
            <MenuHeader
                categories={categories}
                activeCategory={activeCategory}
                onCategoryClick={scrollToSection}
                isAdmin={true}
                onAddItem={handleAddItem}
            />

            <main className="menu-content">

                <div id="classic" className="menu-section">
                    <h2 className="menu-section-title">Classic Coffee</h2>
                    <ItemList
                        items={products.classic}
                        isAdmin={true}
                        onEdit={handleEditProduct}
                        onViewProduct={onViewProduct}
                    />
                </div>

                <div id="frappe" className="menu-section">
                    <h2 className="menu-section-title">Frappe</h2>
                    <ItemList
                        items={products.frappe}
                        isAdmin={true}
                        onEdit={handleEditProduct}
                        onViewProduct={onViewProduct}
                    />
                </div>

                <div id="latte" className="menu-section">
                    <h2 className="menu-section-title">Latte</h2>
                    <ItemList
                        items={products.latte}
                        isAdmin={true}
                        onEdit={handleEditProduct}
                        onViewProduct={onViewProduct}
                    />
                </div>

                <div id="specialty" className="menu-section">
                    <h2 className="menu-section-title">Specialty Drinks</h2>
                    <ItemList
                        items={products.specialty}
                        isAdmin={true}
                        onEdit={handleEditProduct}
                        onViewProduct={onViewProduct}
                    />
                </div>

                <div id="baked" className="menu-section">
                    <h2 className="menu-section-title">Cupcakes & Baked Goods</h2>
                    <ItemList
                        items={products.baked}
                        isAdmin={true}
                        onEdit={handleEditProduct}
                        onViewProduct={onViewProduct}
                    />
                </div>

            </main>

            {editingItem && (
                <EditItemAdmin
                    product={editingItem}
                    onClose={handleCloseEdit}
                    onSave={handleSaveAndClose}
                />
            )}

            <AddItemAdmin
                show={showAddModal}
                handleClose={handleCloseAdd}
                onSave={handleAddSave}
            />
        </div>
    );
};

export default AdminMenu;